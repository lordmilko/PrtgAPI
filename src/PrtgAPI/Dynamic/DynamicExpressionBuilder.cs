using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace PrtgAPI.Dynamic
{
    delegate DynamicMetaObject Fallback(DynamicMetaObject errorSuggestion);

    class DynamicExpressionBuilder<T>
    {
        public static readonly Expression[] NoArgs = new Expression[0]; //Used in reference comparison, requires unique object identity

        public DynamicMetaObject<T> MetaObject { get; set; }

        public DynamicMetaObjectBinder Binder { get; set; }

        public Expression[] Args { get; set; }

        public Fallback Fallback { get; set; }

        public Fallback FallbackInvoke { get; set; }

        public DynamicExpressionBuilder(DynamicMetaObject<T> metaObject, DynamicMetaObjectBinder binder, Expression[] args,
            Fallback fallback, Fallback fallbackInvoke = null)
        {
            MetaObject = metaObject;
            Binder = binder;
            Args = args;
            Fallback = fallback;
            FallbackInvoke = fallbackInvoke;
        }

        public DynamicMetaObject CallMethodWithResult(string methodName)
        {
            var fallbackResult = Fallback(null);

            var callDynamic = BuildCallMethodWithResult(methodName, fallbackResult);

            return Fallback(callDynamic);
        }

        private DynamicMetaObject BuildCallMethodWithResult(string methodName, DynamicMetaObject fallbackResult)
        {
            /* Build a new expression like:
                   object result;
                   TryGetMember(payload, out result) ? fallbackInvoke(result) : fallbackResult */
            var testResult = Expression.Parameter(typeof(object), null);
            var callArgs = Expression.Parameter(typeof(object[]), null);
            var callArgsValue = GetConvertedArgs(Args);

            var methodResult = GetConvertFailedExpression(testResult);

            var callDynamic = new DynamicMetaObject(
                Expression.Block(
                    new[] { testResult, callArgs },
                    Expression.Assign(callArgs, Expression.NewArrayInit(typeof(object), callArgsValue)),
                    GetTernaryExpression(
                        methodName,
                        callArgs,
                        testResult,
                        methodResult.Expression,
                        fallbackResult.Expression,
                        Binder.ReturnType
                    )
                ),
                GetRestrictions().Merge(methodResult.Restrictions).Merge(fallbackResult.Restrictions)
            );

            return callDynamic;
        }

        public DynamicMetaObject CallMethodReturnLast(string methodName, Expression value)
        {
            /* Build an expression like
                   object result;
                   TrySetMember(payload, result = value) ? result : fallbackResult */

            DynamicMetaObject fallbackResult = Fallback(null);                                          //What the binder will do if we don't bind
            
            var trueResult = Expression.Parameter(typeof(object), null);                                //Create an anonymous local variable for storing the result when true
            var testResult = Expression.Assign(trueResult, Expression.Convert(value, typeof (object)));
            var callArgs = Expression.Parameter(typeof(object[]), null);                                //Will eventually store the array of arguments passed to our proxy's Try* Member methods
            var callArgsValue = GetConvertedArgs(Args);

            var callDynamic = new DynamicMetaObject(
                Expression.Block(
                    new[] { trueResult, callArgs },
                    Expression.Assign(callArgs, Expression.NewArrayInit(typeof(object), callArgsValue)),
                    GetTernaryExpression(
                        methodName,
                        callArgs,
                        testResult,
                        trueResult,
                        fallbackResult.Expression,
                        typeof(object)
                    )
                ),
                GetRestrictions().Merge(fallbackResult.Restrictions)
            );

            //
            // Now, call fallback again using our new MetaObject as the error
            // When we do this, one of two things can happen:
            //   1. Binding will succeed, and it will ignore our call to
            //      the dynamic method, OR
            //   2. Binding will fail, and it will use the MetaObject we created
            //      above.
            //
            return Fallback(callDynamic);
        }

        #region Ternary Expressions

        private Expression GetTernaryExpression(string methodName,
            ParameterExpression callArgs, Expression testResult, Expression trueResult, Expression fallback, Type returnType)
        {
            //Construct an If/Else statement that attempts to perform our custom operation. If it fails,
            //we invoke the default behavior of the binder that would have run had we never been here

            var expr = Expression.Condition(
                GetTernaryTest(methodName, callArgs, testResult), //Test
                GetTernaryTrue(callArgs, trueResult),             //True
                fallback,                                         //False
                returnType                                        //Return Type
            );

            return expr;
        }

        private Expression GetTernaryTest(string methodName, ParameterExpression callArgs, Expression result)
        {
            var source = MetaObject.Proxy;
            var methodInfo = source.GetType().GetMethod(methodName);

            if (methodInfo == null)
                throw new ArgumentException($"Could not find method '{methodName}' on {source.GetType().Name}.");

            var args = BuildCallArgs(callArgs, result);

            var method = Expression.Call(
                Expression.Constant(MetaObject.Proxy), //Instance
                methodInfo,                            //Method
                args                                   //Arguments
            );

            return method;
        }

        private Expression GetTernaryTrue(ParameterExpression callArgs, Expression result)
        {
            var expr = Expression.Block(
                ReferenceArgAssign(callArgs, Args),
                result
            );

            return expr;
        }

        #endregion

        [ExcludeFromCodeCoverage]
        private DynamicMetaObject GetConvertFailedExpression(ParameterExpression result)
        {
            var resultMO = new DynamicMetaObject(result, BindingRestrictions.Empty);

            if (Binder.ReturnType != typeof(object))
                throw new NotSupportedException($"Binder {Binder.GetType().Name} is not currently supported.");

            if (FallbackInvoke != null)
                resultMO = FallbackInvoke(resultMO);

            return resultMO;
        }

        #region Expression Helpers

        private Expression GetLimitedSelf()
        {
            // Convert to DynamicObject rather than LimitType, because
            // the limit type might be non-public.
            if (AreEquivalent(MetaObject.Expression.Type, MetaObject.ValueType))
            {
                return MetaObject.Expression;
            }
            return Expression.Convert(MetaObject.Expression, MetaObject.ValueType);
        }

        public static bool AreEquivalent(Type t1, Type t2)
        {
            return t1 == t2 || t1.IsEquivalentTo(t2);
        }

        private static Expression[] GetConvertedArgs(params Expression[] args)
        {
            var paramArgs = new Expression[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                paramArgs[i] = Expression.Convert(args[i], typeof(object));
            }

            return paramArgs;
        }

        private Expression[] BuildCallArgs(Expression arg0, Expression arg1)
        {
            var list = new List<Expression>
            {
                GetLimitedSelf(),
                Constant(Binder)
            };

            if (!ReferenceEquals(Args, NoArgs))
                list.Add(arg0);

            if (arg1 != null)
                list.Add(arg1);

            return list.ToArray();
        }

        private static ConstantExpression Constant(DynamicMetaObjectBinder binder)
        {
            Type t = binder.GetType();

            while (!t.IsVisible)
                t = t.BaseType;

            return Expression.Constant(binder, t);
        }

        [ExcludeFromCodeCoverage]
        private static Expression ReferenceArgAssign(Expression callArgs, Expression[] args)
        {
            ReadOnlyCollectionBuilder<Expression> block = null;

            for (int i = 0; i < args.Length; i++)
            {
                ParameterExpression variable = args[i] as ParameterExpression;
                Requires(variable != null);

                if (variable.IsByRef)
                {
                    if (block == null)
                        block = new ReadOnlyCollectionBuilder<Expression>();

                    block.Add(
                        Expression.Assign(
                            variable,
                            Expression.Convert(
                                Expression.ArrayIndex(
                                    callArgs,
                                    Expression.Constant(i)
                                ),
                                variable.Type
                            )
                        )
                    );
                }
            }

            if (block != null)
                return Expression.Block(block);

            return Expression.Empty();
        }

        [ExcludeFromCodeCoverage]
        private static void Requires(bool precondition)
        {
            if (!precondition)
                throw new ArgumentException("Method precondition violated.");
        }

        private BindingRestrictions GetRestrictions()
        {
            Debug.Assert(MetaObject.Restrictions == BindingRestrictions.Empty, "We don't merge, restrictions are always empty.");

            return GetTypeRestriction(MetaObject);
        }

        private static BindingRestrictions GetTypeRestriction(DynamicMetaObject obj)
        {
            if (obj.Value == null && obj.HasValue)
                return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);

            return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
        }

        #endregion
    }
}
