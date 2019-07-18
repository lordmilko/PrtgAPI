using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Identifies references to <see cref="Query{T}"/> properties with a corresponding <see cref="PropertyExpression"/>.
    /// References are traced through intermediate transformations (such as anonymous types) back to their original sources.
    /// </summary>
    /// <typeparam name="T">The type of object to evaluate properties for.</typeparam>
    class PropertyEvaluator<T> : ExpressionVisitor
    {
        /// <summary>
        /// Maps a parameter passed to a Select method to a <see cref="MemberInitExpression"/> or <see cref="NewExpression"/> used to instantiate
        /// the incoming object.
        /// </summary>
        private Dictionary<ParameterExpression, Expression> parameterInitializer = new Dictionary<ParameterExpression, Expression>();

        private object cacheLock = new object();

        /// <summary>
        /// The property cache of this <typeparamref name="T"/>. Each <typeparamref name="T"/> has its own property cache.
        /// </summary>
        private static ReadOnlyCollection<PropertyCache> propertyCache;

        private static ConcurrentDictionary<PropertyInfo, PropertyCache> internalPropertyMap = new ConcurrentDictionary<PropertyInfo, PropertyCache>();

        private bool strict;

        public PropertyEvaluator(bool strict)
        {
            this.strict = strict;
        }

        public LambdaExpression Evaluate(Expression source, LambdaExpression selector)
        {
            Logger.Log($"Identifying source expression of parameter '{selector.Parameters[0]}' in expression '{selector}'", Indentation.Three);

            var sourceExpr = CreateInitializer(source);

            Logger.Log($"Identified parameter source '{sourceExpr}'", Indentation.Four);

            parameterInitializer[selector.Parameters[0]] = sourceExpr;

            return (LambdaExpression) Visit(selector);
        }

        private Expression CreateInitializer(Expression source)
        {
            if (source is ConstantExpression && ((ConstantExpression)source).Value is Query<T>)
                return CreateSourceExpression();
            else
                return CreateIntermediateExpression(source);
        }

        private Expression CreateSourceExpression()
        {
            lock (cacheLock)
            {
                if (propertyCache == null)
                {
                    //We can't store set-only properties in a MemberInitExpression, but we can in a New expression!
                    //As such, we store the internal, settable member of this property in a map. When we need to resolve
                    //a member used in an anonymous type, we swap our internal member out with our public one, and then use
                    //the public one in all NewExpression's used to represent anonymous types from there on out
                    var properties = GetProperties();
                    var settable = properties.Where(p => p.Property.GetSetMethod() != null).ToList();
                    var notSettable = properties.Where(p => p.Property.GetSetMethod() == null).ToList();

                    var internalProperties = typeof(T).GetTypeCache().Properties.Where(p => p.Property.CanWrite && p.Property.GetSetMethod() == null).ToArray();

                    var internalSettable = notSettable.Select(p => GetInternalProperty(p, internalProperties)).ToList();

                    settable.AddRange(internalSettable);

                    propertyCache = settable.AsReadOnly();
                }
            }

            var list = propertyCache.Select(p => Expression.Bind(p.Property, new PropertyExpression(p.Property))).ToList();

            var init = Expression.MemberInit(Expression.New(typeof(T)), list);

            return init;
        }

        private PropertyCache GetInternalProperty(PropertyCache cache, PropertyCache[] internalProperties)
        {
            var property = (Property)cache.GetAttribute<PropertyParameterAttribute>().Property;

            var description = property.GetDescription().ToLower();

            var prop = internalProperties.First(p =>
            {
                var name = p.GetAttributes<XmlElementAttribute>().FirstOrDefault()?.ElementName;
                return name == description || name == $"{description}_raw";
            });

            internalPropertyMap[prop.Property] = cache;

            return prop;
        }

        private Expression CreateIntermediateExpression(Expression source)
        {
            if (source is LinqExpression)
            {
                if (source is SelectLinqExpression)
                    return CreateSelector((SelectLinqExpression)source, s => s.Selector);

                if (source is SelectManyLinqExpression)
                    return CreateSelector((SelectManyLinqExpression) source, s =>
                    {
                        if (s.ResultSelector != null)
                            return s.ResultSelector;

                        return s.CollectionSelector;
                    });

                return CreateInitializer(((LinqExpression) source).Source);
            }

            if (source is MethodCallExpression) //We're probably an unsupported LINQ expression
            {
                var call = (MethodCallExpression)source;

                if (call.Arguments.Count > 0)
                    return CreateInitializer(call.Arguments[0]);
            }

            throw new NotImplementedException($"Don't know how to create intermediate expression from {source.GetType()}.");
        }

        private Expression CreateSelector<TSource>(TSource select, Func<TSource, LambdaExpression> getSelector)
            where TSource : SelectionLinqExpression
        {
            var selector = getSelector(select);

            var result = Visit(selector.Body);

            return result;
        }

        private List<PropertyCache> GetProperties()
        {
            var types = new List<Type>();

            //Expression trees ignore the reflected type of a property, causing properties to appear to have been derived from their base classes.
            //In order to compare our properties against these, we individually reflect on each base type in our hierarchy
            var baseType = typeof(T);

            while (baseType != null && baseType != typeof(object))
            {
                types.Add(baseType);

                baseType = baseType.BaseType;
            }

            var properties = types.SelectMany(t => t.GetTypeCache().Properties.Where(p =>
                p.GetAttribute<UndocumentedAttribute>() == null &&
                p.GetAttribute<PropertyParameterAttribute>() != null)).ToList();

            return properties;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var source = Visit(node.Expression);

            source = ExpressionHelpers.UnwrapCast(source);

            Expression expr;

            //Upon visiting the MemberExpression, did we transform it into a source MemberInitExpression or
            //intermediate NewExpression?
            switch (source.NodeType)
            {
                case ExpressionType.MemberInit:
                    if (PropertyExpressionFromInit(node, source, out expr))
                        return expr;
                    break;

                case ExpressionType.New: //Analyzing the properties from the previous Select
                    if (PropertyExpressionFromPrevious(node, source, out expr))
                        return expr;
                    break;
            }

            if (source == node.Expression)
                return node;

            if (source.NodeType == ExpressionType.NewArrayInit || source.NodeType == ExpressionType.ListInit)
                return PropertyExpressionFromArray(node, source);

            //We're an unsupported property on the root. Return as is
            if (source.NodeType == ExpressionType.MemberInit)
            {
                if (strict)
                    throw Error.UnsupportedFilterProperty(node);

                return node;
            }

            //We might be a sub-member of a real property. Our property came into this method as p.Prop, but
            //has been transformed to <root>.Prop, so we need to make a new MemberExpression from it
            return Expression.Property(source, (PropertyInfo)node.Member);
        }

        private Expression PropertyExpressionFromArray(MemberExpression node, Expression source)
        {
            var exprs = ExpressionSearcher.Search<NewExpression>(source);

            Expression expr;

            if (exprs.Cast<NewExpression>().All(e => e.Members == null))
            {
                var memberInit = ExpressionSearcher.Search<MemberInitExpression>(source);

                if (memberInit.Count > 0)
                    exprs = memberInit;

                foreach (var e in exprs)
                {
                    if (PropertyExpressionFromInit(node, e, out expr))
                        return expr;
                }
            }
            else
            {
                foreach (var e in exprs)
                {
                    if (PropertyExpressionFromPrevious(node, e, out expr))
                        return expr;
                }
            }

            throw new InvalidOperationException($"Failed to find member '{node}' in source expression array. This should be impossible.");
        }

        private bool IsLiteralMemberInitExpression(MemberInitExpression init, Expression source)
        {
            //That is to say, we are not the MemberInitExpression defined in method CreateSourceExpression()
            if (init.Type != typeof(T))
                return true;

            //We ARE a MemberInitExpression of type T, but are we a REAL PropertyExpression with a backing MemberExpression,
            //or a fake one used to perform property lookups.
            if (ExpressionSearcher.Search(source, e => e is PropertyExpression).Cast<PropertyExpression>()
                .Any(p => p.Expression != null))
            {
                //We're a MemberInitExpression for type T. If we did new Sensor { Message = <root>.Name }.Message,
                //continuing with normal MemberInitExpression parsing logic would reduce us to <root>.Message,
                //eliminating the reference to <root>.Name. As such, we will need to perform a lookup on the
                //assignment "source", which will allow us to determine the whole thing can in fact be replaced
                //with <root>.Name
                return true;
            }

            return false;
        }

        private bool PropertyExpressionFromInit(MemberExpression node, Expression source, out Expression result)
        {
            var init = (MemberInitExpression)source;

            if (IsLiteralMemberInitExpression(init, source))
                return PropertyExpressionFromIntermediate(node, source, out result);         

            foreach (var member in init.Bindings)
            {
                var assignment = member as MemberAssignment;

                if (assignment != null)
                {
                    PropertyCache publicProperty;

                    if (assignment.Member is PropertyInfo && internalPropertyMap.TryGetValue((PropertyInfo)assignment.Member, out publicProperty))
                    {
                        if (MembersMatch(publicProperty.Property, node.Member))
                        {
                            result = new PropertyExpression(node, publicProperty.Property);
                            return true;
                        }
                    }

                    if (MembersMatch(assignment.Member, node.Member) || InterfaceMembersMatch(assignment.Member, node.Member))
                    {
                        //Replace the MemberExpression for this member with its equivalent PropertyExpression
                        result = new PropertyExpression(node, (PropertyInfo) assignment.Member);
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        private bool PropertyExpressionFromPrevious(MemberExpression node, Expression source, out Expression result)
        {
            var newExpr = (NewExpression)source;

            if (newExpr.Members == null)
                return PropertyExpressionFromIntermediate(node, source, out result);

            for (var i = 0; i < newExpr.Members.Count; i++)
            {
                if (MembersMatch(newExpr.Members[i], node.Member))
                {
                    var target = newExpr.Arguments[i];

                    if (target is PropertyExpression)
                        result = new PropertyExpression(node, ((PropertyExpression) newExpr.Arguments[i]).PropertyInfo);
                    else
                    {
                        //It's a ConstantExpression, intermediate NewExpression or something else we can't deal with.
                        //Leave it alone. Note that we're ok with having an intermediate NewExpression; it's not something
                        //that violates strict
                        result = target;
                    }

                    return true;
                }
            }

            result = null;
            return false;
        }

        private bool PropertyExpressionFromIntermediate(MemberExpression node, Expression source,
            out Expression result)
        {
            var init = source as MemberInitExpression;

            if (init?.NewExpression.Arguments.Count == 0)
            {
                foreach (var member in init.Bindings)
                {
                    var assignment = member as MemberAssignment;

                    if (assignment != null)
                    {
                        if (MembersMatch(member.Member, node.Member))
                        {
                            result = assignment.Expression;
                            return true;
                        }
                    }
                }
            }

            if (strict)
                throw Error.AmbiguousPropertyExpression(node);

            //The root PropertyExpression could have been mapped to any property on this type.
            //We cannot perform any further evaluation, so just leave the expression as is
            //(causing it to be ignored)
            result = node;
            return true;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression init;

            if (parameterInitializer.TryGetValue(node, out init))
                return init;

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
            {
                var expr = Visit(node.Operand);

                if (expr != node.Operand)
                {
                    try
                    {
                        return Expression.MakeUnary(node.NodeType, expr, node.Type, node.Method);
                    }
                    catch (InvalidOperationException)
                    {
                        return Expression.MakeUnary(node.NodeType, Expression.Convert(expr, typeof(object)), node.Type, node.Method);
                    }
                }
            }

            return base.VisitUnary(node);
        }

        protected override Expression VisitLambda<TNode>(Expression<TNode> lambda)
        {
            var body = Visit(lambda.Body);

            if (body != lambda.Body)
            {
                var substituteBody = new SubstituteExpression(lambda.Body, body);

                return Expression.Lambda(lambda.Type, substituteBody, lambda.Parameters);
            }

            return lambda;
        }

        private bool MembersMatch(MemberInfo first, MemberInfo second)
        {
            if (first == second)
                return true;

            if (first is MethodInfo && second is PropertyInfo)
                return first == ((PropertyInfo) second).GetGetMethod();

            if (first is PropertyInfo && second is MethodInfo)
                return ((PropertyInfo)first).GetGetMethod() == second;

            return false;
        }

        private bool InterfaceMembersMatch(MemberInfo first, MemberInfo second)
        {
            //Potentially our expression s => s.Name == "blah" was converted to
            //s => ((IObject)s).Name == "blah", either via an explicit cast or
            //autoamtically by the compiler due to type constraints
            if (second.DeclaringType.IsInterface)
            {
                if (first is PropertyInfo && second is PropertyInfo)
                {
                    var objectGet = ((PropertyInfo)first).GetGetMethod();
                    var interfaceGet = ((PropertyInfo)second).GetGetMethod();

                    var interfaceMap = first.ReflectedType.GetInterfaceMap(second.DeclaringType);

                    for (var i = 0; i < interfaceMap.TargetMethods.Length; i++)
                    {
                        if (interfaceMap.TargetMethods[i] == objectGet)
                        {
                            if (interfaceMap.InterfaceMethods[i] == interfaceGet)
                                return true;

                            return false;
                        }
                    }
                }
            }

            return false;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node.CanReduce)
                return base.VisitExtension(node);

            return node;
        }
    }
}
