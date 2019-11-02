using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Expr = System.Linq.Expressions.Expression;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class BinaryExpressionTests : BaseExpressionTest
    {
        #region Supported

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Add() => Execute(s => s.Id + 3 == 5, string.Empty, ExpressionType.Add);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_AddChecked() => ExecuteBinaryExpr(Property.Id, id => Expr.AddChecked(id, Expr.Constant(3)), ExpressionType.AddChecked);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_And() => Execute(s => (s.Id & 2) == 3, string.Empty, ExpressionType.And);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_AndAlso() => Execute(s => s.Id == 4001 && s.ParentId == 0, "filter_objid=4001&filter_parentid=0", ExpressionType.AndAlso);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_ArrayIndex() => Execute(s => s.Tags[0] == "test", "filter_tags=test", ExpressionType.ArrayIndex);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Coalesce() => Execute(s => (s.LastUp ?? DateTime.Now) == DateTime.Now, string.Empty, ExpressionType.Coalesce);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Divide() => Execute(s => s.Id /2 == 2000, string.Empty, ExpressionType.Divide);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Equal() => Execute(s => s.Active == true, "filter_active=-1", ExpressionType.Equal);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_ExclusiveOr() => ExecuteBinaryExpr(Property.Id, id => Expr.ExclusiveOr(id, Expr.Constant(2)), ExpressionType.ExclusiveOr);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_GreaterThan() => Execute(s => s.Id > 3000, "filter_objid=@above(3000)", ExpressionType.GreaterThan);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_GreaterThanOrEqual() => Execute(s => s.Id >= 3000, "filter_objid=@above(3000)&filter_objid=3000", ExpressionType.GreaterThanOrEqual);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_LeftShift() => Execute(s => s.Id << 3 == 20, string.Empty, ExpressionType.LeftShift);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_LessThan() => Execute(s => s.Id < 5000, "filter_objid=@below(5000)", ExpressionType.LessThan);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_LessThanOrEqual() => Execute(s => s.Id <= 5000, "filter_objid=@below(5000)&filter_objid=5000", ExpressionType.LessThanOrEqual);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Modulo() => Execute(s => s.Id % 2 == 0, string.Empty, ExpressionType.Modulo);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Multiply() => Execute(s => s.Id * s.Id == 3, string.Empty, ExpressionType.Multiply);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_MultiplyChecked() => ExecuteBinaryExpr(Property.Id, id => Expr.MultiplyChecked(id, Expr.Constant(3)), ExpressionType.MultiplyChecked);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_NotEqual() => Execute(s => s.Id != 4000, "filter_objid=@neq(4000)", ExpressionType.NotEqual);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Or() => Execute(s => (s.Id | 3) == 2, string.Empty, ExpressionType.Or);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_OrElse() => Execute(s => s.Id == 4000 || s.Id == 4001, "filter_objid=4000&filter_objid=4001", ExpressionType.OrElse);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Power()
        {
            ExecuteBinaryExpr(Property.Id, id =>
            {
                var pow = Expr.Convert(Expr.Power(Expr.Convert(id, typeof(double)), Expr.Constant(2.0)), typeof(int));

                return pow;
            }, ExpressionType.Power);
        }

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_RightShift() => Execute(s => s.Id >> 2 == 5, string.Empty, ExpressionType.RightShift);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Subtract() => Execute(s => s.Id - 5 == 3, string.Empty, ExpressionType.Subtract);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_SubtractChecked()
        {
            ExecuteExpr(Property.Id, id =>
            {
                var subtract = Expr.SubtractChecked(id, Expr.Constant(3));

                return Expr.Equal(subtract, Expr.Constant(2));
            }, ExpressionType.SubtractChecked);
        }

        #endregion
        #region Unsupported

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_Assign() => UnsupportedBinaryExpr(Property.Id, id => Expr.Assign(id, Expr.Constant(3)), ExpressionType.Assign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_AddAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.AddAssign(id, Expr.Constant(3)), ExpressionType.AddAssign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_AndAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.AndAssign(id, Expr.Constant(3)), ExpressionType.AndAssign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_DivideAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.DivideAssign(id, Expr.Constant(3)), ExpressionType.DivideAssign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_ExclusiveOrAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.ExclusiveOrAssign(id, Expr.Constant(3)), ExpressionType.ExclusiveOrAssign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_LeftShiftAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.LeftShiftAssign(id, Expr.Constant(3)), ExpressionType.LeftShiftAssign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_ModuloAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.ModuloAssign(id, Expr.Constant(3)), ExpressionType.ModuloAssign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_MultiplyAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.MultiplyAssign(id, Expr.Constant(3)), ExpressionType.MultiplyAssign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_OrAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.OrAssign(id, Expr.Constant(3)), ExpressionType.OrAssign);

        static int Pow(int x, int y)
        {
            return (int) Math.Pow((double) x, (double) y);
        }

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_PowerAssign()
        {
            UnsupportedBinaryExpr(Property.Id, id =>
            {
                var method = GetType().GetMethod("Pow", BindingFlags.NonPublic | BindingFlags.Static);

                var assign = Expr.PowerAssign(id, Expr.Constant(3), method);

                return assign;
            }, ExpressionType.PowerAssign);
        }

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_RightShiftAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.RightShiftAssign(id, Expr.Constant(3)), ExpressionType.RightShiftAssign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_SubtractAssign() => UnsupportedBinaryExpr(Property.Id, id => Expr.SubtractAssign(id, Expr.Constant(3)), ExpressionType.SubtractAssign);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_AddAssignChecked() => UnsupportedBinaryExpr(Property.Id, id => Expr.AddAssignChecked(id, Expr.Constant(3)), ExpressionType.AddAssignChecked);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_MultiplyAssignChecked() => UnsupportedBinaryExpr(Property.Id, id => Expr.MultiplyAssignChecked(id, Expr.Constant(3)), ExpressionType.MultiplyAssignChecked);

        [UnitTest]
        [TestMethod]
        public void BinaryExpression_SubtractAssignChecked() => UnsupportedBinaryExpr(Property.Id, id => Expr.SubtractAssignChecked(id, Expr.Constant(3)), ExpressionType.SubtractAssignChecked);

        #endregion
    }
}
