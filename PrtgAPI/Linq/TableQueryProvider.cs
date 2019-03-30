using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PrtgAPI.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq
{
    class TableQueryProvider<TObject, TParam> : QueryProvider
        where TObject : ITableObject, IObject
        where TParam : TableParameters<TObject>
    {
        private Func<TParam> createParameters;

        /// <summary>
        /// Specifies that PrtgAPI should throw exceptions upon encountering unsupported expressions, rather than executing client side.
        /// </summary>
        private bool strict;
        private QueryHelper<TObject> queryHelper;
        private Func<TParam, IEnumerable<TObject>> streamer;

        public TableQueryProvider(Func<TParam> createParameters, bool strict, QueryHelper<TObject> queryHelper, Func<TParam, IEnumerable<TObject>> streamer)
        {
            this.createParameters = createParameters;
            this.strict = strict;
            this.queryHelper = queryHelper;
            this.streamer = streamer;
        }

        public override object Execute(Expression expression)
        {
            Logger.Log($"Executing expression '{expression}'. Strict: {strict}");

            var parameterBuilder = new ParameterBuilder<TObject>(queryHelper, strict);

            //Validate that every tree node is supported
            var supportedTree = SupportedExpressionValidator.Validate(expression);

            //Replace variable and method references with their calculated values
            var evaluatedTree = PartialEvaluate(supportedTree);

            //Replace all expressions that correspond to LINQ methods with LinqExpression objects
            var linqTree = LinqQueryBuilder<TObject>.Parse(evaluatedTree, strict);

            //Merge sequential method calls together into a single statement (e.g. Where(A).Where(B) => Where(A && B))
            var mergedTree = LinqQueryMerger.Parse(linqTree, strict);

            //Extract the Parameter details from each LinqExpression, replacing processed expressions with their TSource
            var legalTree = parameterBuilder.Build(mergedTree);

            //Construct the parameter objects required to execute all of the required requests
            var parameterSets = GetParameters(parameterBuilder);

            //Filter out server side only expressions so we don't execute them twice (such as Skip)
            var clientTree = ClientTreeBuilder.Parse(linqTree);

            //Convert the query to LINQ to Objects
            var enumerableTree = RewriteEnumerable(clientTree, parameterSets);

            var getResults = Expression.Lambda(enumerableTree).Compile();

            if (getResults.Method.ReturnType.IsValueType)
            {
                try
                {
                    return getResults.DynamicInvoke();
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }

            return ((Func<object>)getResults)();
        }

        private Expression RewriteEnumerable(Expression expr, List<TParam> parameterSets)
        {
            var rewriter = new EnumerableRewriter<TObject>(new QueryStreamer<TObject, TParam>(streamer, parameterSets, queryHelper));

            var result = rewriter.Visit(expr);

            return result;
        }

        private Expression PartialEvaluate(Expression expression)
        {
            Logger.Log($"Partial evaluating expression '{expression}'", Indentation.One);
            var expressionAnalyzer = new LocalExpressionAnalyzer(CanBeEvaluatedLocally);

            Logger.Log("Identifying candidates for partial evaluation", Indentation.Two);
            var candidates = expressionAnalyzer.GetLocalExpressions(expression).ToList();

            Logger.Log($"Identified {candidates.Count} candidates for local evaluation", Indentation.Two, candidates);
            ValidateCandidates(expression, candidates);

            var partialEvaluated = new LocalExpressionEvaluator(candidates).Evaluate(expression);

            Logger.Log($"Reduced expression to '{partialEvaluated}'", Indentation.Two);

            return partialEvaluated;
        }

        private void ValidateCandidates(Expression expression, List<Expression> candidates)
        {
            List<Expression> toRemove = new List<Expression>();

            foreach (var candidate in candidates)
            {
                if (toRemove.Contains(candidate))
                    continue;

                if (typeof(IEnumerable).IsAssignableFrom(candidate.Type) && candidate.NodeType == ExpressionType.New)
                {
                    var parents = ExpressionSearcher.GetParents(candidate, expression);

                    var listInit = parents.Where(p => p.NodeType == ExpressionType.ListInit).ToList();

                    if (!CanEvaluateLocally(listInit))
                    {
                        toRemove.Add(candidate);

                        foreach (var c in candidates)
                        {
                            if (parents.Any(p => p == c))
                                toRemove.AddRange(parents.Where(p => p == c));
                        }
                    }
                }
            }

            if (toRemove.Count > 0)
            {
                Logger.Log($"Removing {toRemove.Count} incorrectly flagged candidates", Indentation.Two, toRemove);

                foreach (var expr in toRemove)
                    candidates.Remove(expr);
            }
        }

        private bool CanEvaluateLocally(List<Expression> listInits)
        {
            foreach (var init in listInits)
            {
                var listChildren = ExpressionSearcher.Search(init, e => true);

                if (listChildren.Any(c => !CanBeEvaluatedLocally(c)))
                    return false;
            }

            return true;
        }

        private bool CanBeEvaluatedLocally(Expression expr)
        {
            var constant = expr as ConstantExpression;

            var query = constant?.Value as IQueryable;

            if (query != null && query.Provider == this)
                return false;

            var method = expr as MethodCallExpression;

            if (method != null && ExpressionHelpers.IsQueryMethod(method))
                return false;

            if (expr.NodeType == ExpressionType.Convert && expr.Type == typeof(object))
                return true;

            return expr.NodeType != ExpressionType.Parameter && expr.NodeType != ExpressionType.Lambda;
        }

        private List<TParam> GetParameters(ParameterBuilder<TObject> builder)
        {
            if (builder.FilterSets.Count == 0)
                builder.FilterSets.Add(new List<SearchFilter>());

            Logger.Log($"Building parameters from {builder.FilterSets.Count} filter sets", Indentation.One);
            
            var parameterSets = builder.FilterSets.Select(f =>
            {
                var @params = createParameters();

                if (f.Count > 0)
                    @params.SearchFilters = f;

                if (builder.Properties.Count > 0)
                {
                    if (!builder.Properties.Contains(Property.Id) && builder.FilterSets.Count > 1)
                        builder.Properties.Add(Property.Id); //Must include Id for DistinctBy

                    //IEventObject types like Logs can't limit the properties returned when attempting to DistinctBy
                    //across multiple requests as the presence of every property is required to tell
                    //whether or not the record is distinct
                    if (queryHelper == null || queryHelper.CanLimitProperties(builder.FilterSets))
                        @params.Properties = builder.Properties.OrderBy(p => p != Property.Id).ToArray();
                }

                if (builder.Skip != null)
                    @params.Start = builder.Skip.Value + @params.StartOffset;

                if (builder.SortProperty != null)
                {
                    @params.SortBy = builder.SortProperty.Value;
                    @params.SortDirection = builder.SortDirection.Value;
                }

                //how to inject take iterator mid stream?

                if (builder.Count != null && !builder.HasIllegalServerFilters)
                    @params.Count = builder.Count;

                queryHelper?.FixupParameters(@params);

                return @params;
            }).ToList();

            return parameterSets;
        }
    }
}
