using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace PrtgAPI.Request.Serialization.CodeGen
{
#if DEBUG && DEBUG_SERIALIZATION
    class ProxyExpression
    {
        internal static ParameterExpression typeBuilderParameter = Expression.Parameter(typeof(TypeBuilder), "typeBuilder");

        public static Expression ToMethodCallExpression(InvocationExpression invocation)
        {
            var lambda = invocation.Expression as LambdaExpression;

            if (lambda == null)
                throw new NotImplementedException($"Don't know how to convert expression of type '{invocation.Expression.NodeType}' to method");

            invocation = UpdateInvocation(invocation, lambda);

            Type targetType = invocation.Expression.Type;

            if (targetType.IsGenericType)
                targetType = targetType.GetGenericTypeDefinition();

            var genericProxy = ProxyFactory.Get(targetType);

            var proxyType = genericProxy.MakeGenericType(invocation.Expression.Type.GetGenericArguments());

            var method = proxyType.GetMethod("Invoke");

            var proxyCtor = proxyType.GetConstructor(new[] { typeof(TypeBuilder), typeof(string) });

            var proxy = Expression.New(proxyCtor, invocation.Arguments.Last(), Expression.Constant(lambda.Name));

            var methodCall = Expression.Call(proxy, method, invocation.Arguments);
            return methodCall;
        }

        private static InvocationExpression UpdateInvocation(InvocationExpression i, LambdaExpression lambda)
        {
            var lambdaArgs = lambda.Parameters.ToList();
            lambdaArgs.Add(typeBuilderParameter);
            lambda = Expression.Lambda(lambda.Body, lambdaArgs);

            var invocationArgs = i.Arguments.ToList();
            invocationArgs.Add(typeBuilderParameter);

            i = i.Update(lambda, invocationArgs);

            return i;
        }
    }
#endif
}
