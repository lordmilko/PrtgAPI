using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Attributes;
using PrtgAPI.Linq.Expressions.Visitors;
using PrtgAPI.Tests.UnitTests.Support;
using Expr = System.Linq.Expressions.Expression;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    public abstract class BaseExpressionTest : BaseQueryTests
    {
        protected void Execute(Expression<Func<Sensor, bool>> predicate, string url, ExpressionType nodeType)
        {
            var types = ExpressionSearcher.GetTypes(predicate);

            Assert.IsTrue(types.Any(t => t == nodeType), $"Did not have an expression of type {nodeType}. Types found: " + string.Join(", ", types));

            var urls = new[]
            {
                UnitRequest.Sensors($"count=500" + (string.IsNullOrEmpty(url) ? url : $"&{url}"), UrlFlag.Columns)
            };

            var client = GetClient(urls.ToArray());

            var result = client.Item1.QuerySensors().Where(predicate).ToList();

            client.Item2.AssertFinished();
        }

        protected void ExecuteBinaryExpr(Property property, Func<Expr, Expr> expr, ExpressionType nodeType, string url = "")
        {
            ExecuteExpr(property, v => Expr.Equal(expr(v), Expr.Constant(3)), nodeType, url);
        }

        protected void ExecuteExpr(Property property, Func<Expr, Expr> expr, ExpressionType nodeType, string url = "")
        {
            var lambda = CreateLambda(property, expr);

            Execute(lambda, url, nodeType);
        }

        public static Expression<Func<Sensor, bool>> CreateLambda(Property property, Func<Expr, Expr> expr)
        {
            var prop = typeof(Sensor)
                .GetProperties().First(e => e.GetCustomAttributes(typeof(PropertyParameterAttribute), false)
                    .Cast<PropertyParameterAttribute>()
                    .Any(p => p.Property.Equals(property)));

            var parameter = Expr.Parameter(typeof(Sensor), "s");

            var member = Expr.MakeMemberAccess(parameter, prop);

            var lambda = Expr.Lambda<Func<Sensor, bool>>(
                expr(member),
                parameter
            );

            return lambda;
        }

        protected void UnsupportedBinaryExpr(Property property, Func<Expr, Expr> expr, ExpressionType nodeType)
        {
            UnsupportedExpr(property, v => Expr.Equal(expr(v), Expr.Constant(3)), nodeType);
        }

        protected void UnsupportedExpr(Property property, Func<Expr, Expr> expr, ExpressionType nodeType)
        {
            AssertEx.Throws<InvalidOperationException>(() => ExecuteExpr(property, expr, nodeType), $"Cannot process specified expression: node type '{nodeType}' is not supported.");
        }
    }
}
