using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Expr = System.Linq.Expressions.Expression;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class UnaryExpressionTests : BaseExpressionTest
    {
        #region Supported

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_ArrayLength() => Execute(s => s.Tags.Length == 3, string.Empty, ExpressionType.ArrayLength);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_Convert() => Execute(s => (double)s.Id == 3.0, "filter_objid=3", ExpressionType.Convert);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_ConvertChecked() => ExecuteBinaryExpr(Property.Active, active => Expr.ConvertChecked(active, typeof(int)), ExpressionType.ConvertChecked);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_Decrement() => ExecuteBinaryExpr(Property.Id, Expr.Decrement, ExpressionType.Decrement);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_Increment() => ExecuteBinaryExpr(Property.Id, Expr.Increment, ExpressionType.Increment);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_IsTrue() => ExecuteExpr(Property.Active, active => Expr.Equal(active, Expr.IsTrue(Expr.Constant(true))), ExpressionType.IsTrue, "filter_active=-1");

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_IsFalse() => ExecuteExpr(Property.Active, active => Expr.Equal(active, Expr.IsFalse(Expr.Constant(false))), ExpressionType.IsFalse, "filter_active=-1");

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_Negate() => Execute(s => -s.Id == 4000, string.Empty, ExpressionType.Negate);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_NegateChecked() => ExecuteBinaryExpr(Property.Id, Expr.NegateChecked, ExpressionType.NegateChecked);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_Not() => Execute(s => !s.Active, "filter_active=0", ExpressionType.Not);

        /*[UnitTest]
        [TestMethod]
        public void UnaryExpression_Quote()
        {
            ExecuteExpr(Property.Id, id => Expr.TypeIs(Expr.Constant(GetClient(new[]
            {
                RequestDeviceCount,
                RequestDevice()
            }).QueryDevices().Select(s => s.Id)), typeof(int)), ExpressionType.Quote);
        }*/

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_TypeAs() => Execute(s => ((object)s.Id) as int? == 3, "filter_objid=3", ExpressionType.TypeAs);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_UnaryPlus() => ExecuteBinaryExpr(Property.Id, Expr.UnaryPlus, ExpressionType.UnaryPlus, "filter_objid=3");

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_Unbox() => ExecuteExpr(Property.LastUp, lastUp =>
        {
            var unbox = Expr.Unbox(Expr.Convert(lastUp, typeof(object)), typeof(DateTime));

            return Expr.Equal(unbox, Expr.Constant(new DateTime(2000, 10, 2, 12, 10, 5, DateTimeKind.Utc)));
        }, ExpressionType.Unbox, "filter_lastup=36801.5070023148");

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_OnesComplement() => ExecuteBinaryExpr(Property.Id, Expr.OnesComplement, ExpressionType.OnesComplement);

        #endregion
        #region Unsupported

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_PreIncrementAssign() => UnsupportedBinaryExpr(Property.Id, Expr.PreIncrementAssign, ExpressionType.PreIncrementAssign);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_PreDecrementAssign() => UnsupportedBinaryExpr(Property.Id, Expr.PreDecrementAssign, ExpressionType.PreDecrementAssign);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_PostIncrementAssign() => UnsupportedBinaryExpr(Property.Id, Expr.PostIncrementAssign, ExpressionType.PostIncrementAssign);

        [UnitTest]
        [TestMethod]
        public void UnaryExpression_PostDecrementAssign() => UnsupportedBinaryExpr(Property.Id, Expr.PostDecrementAssign, ExpressionType.PostDecrementAssign);

        //C# 7 language feature

        /*[UnitTest]
        [TestMethod]
        public void UnaryExpression_Throw() => UnsupportedExpr(Property.Id, id =>
        Expr.Condition(
            Expr.Constant(true),
            Expr.Constant(true),
            Expr.Throw(Expr.New(typeof(Exception)))
        ), ExpressionType.Throw);*/

        #endregion
    }
}
