using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Expr = System.Linq.Expressions.Expression;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class MiscellaneousExpressionTests : BaseExpressionTest
    {
        #region Supported

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Call() => Execute(s => s.LastUp.Value.AddDays(1).Ticks == 3, string.Empty, ExpressionType.Call);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Conditional() => Execute(s => s.Id > 3 ? 1 > 2 : 2 < 1, string.Empty, ExpressionType.Conditional);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Constant() => Execute(s => s.Id == 3, "filter_objid=3", ExpressionType.Constant);

        /*[TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_DebugInfo()
        {
            var doc = Expr.SymbolDocument("PrtgClient.cs");
            Expr.DebugInfo()

            Execute(s => s.Id == 3, string.Empty, ExpressionType.DebugInfo);
        }*/

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Default() => ExecuteExpr(Property.Id, id =>
        {
            var d = Expr.Default(typeof(int));
            return Expr.Equal(id, d);
        }, ExpressionType.Default, "filter_objid=0");

        //[TestMethod]
        //public void MiscellaneousExpression_Extension() => Execute(s => s.Id == 3, string.Empty, ExpressionType.Extension);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Index()
        {
            var list = Enumerable.Range(0, 5000).ToList();

            var indexer = list.GetType().GetProperty("Item");

            ExecuteBinaryExpr(Property.Id, id =>
            {
                var index = Expr.MakeIndex(Expr.Constant(list), indexer, new[] {id});

                return index;
            }, ExpressionType.Index);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Invoke()
        {
            Func<Sensor, bool> lambda = s => s.Name == "Volume IO _Total1";

            Execute(s => lambda(s), string.Empty, ExpressionType.Invoke);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Lambda() => Execute(s => s.Id == 3, "filter_objid=3", ExpressionType.Lambda);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_ListInit() => Execute(s => s.Name == new List<string> {"test"}.First(), "filter_name=test", ExpressionType.ListInit);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_MemberAccess() => Execute(s => s.Id == 3, "filter_objid=3", ExpressionType.MemberAccess);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_MemberInit() => Execute(s =>
            new RealTypeProperty { RealName = s.Name }.RealName == "test", 
            "filter_name=test",
            ExpressionType.MemberInit
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_New() => Execute(s => s.Name == new string(new[] {'a'}), "filter_name=a", ExpressionType.New);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_NewArrayInit() => Execute(s => s.Name == new string(new[] { 'a' }), "filter_name=a", ExpressionType.NewArrayInit);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_NewArrayBounds() => ExecuteExpr(Property.Tags, tags =>
        {
            var ar = Expr.NewArrayBounds(typeof(string), Expr.Constant(3));

            return Expr.Equal(tags, ar);
        }, ExpressionType.NewArrayBounds);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Parameter() => Execute(s => s.Id == 3, "filter_objid=3", ExpressionType.Parameter);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_TypeIs() => Execute(s => s is Sensor, string.Empty, ExpressionType.TypeIs);

        #endregion
        #region Unsupported

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Block()
        {
            var variable = Expr.Variable(typeof(bool));

            var block = Expr.Block(Expr.Assign(variable, Expr.Constant(true)), variable);

            UnsupportedExpr(Property.Active, active => Expr.Equal(active, block), ExpressionType.Block);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Switch()
        {
            var switchCase = Expr.SwitchCase(Expr.Constant(true), Expr.Constant(4001));

            UnsupportedExpr(Property.Id, id => Expr.Switch(id, Expr.Constant(false), switchCase), ExpressionType.Switch);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_Try()
        {
            var tryCatch = Expr.TryCatch(Expr.Constant(false), Expr.Catch(typeof(Exception), Expr.Constant(false)));

            UnsupportedExpr(Property.Id, id => tryCatch, ExpressionType.Try);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MiscellaneousExpression_TypeEqual()
        {
            ExecuteExpr(Property.Id, id => Expr.TypeEqual(id, typeof(Sensor)), ExpressionType.TypeEqual);
        }

        #endregion

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ExpressionType_TestAllTypes()
        {
            var types = typeof(ExpressionType).GetEnumValues().Cast<ExpressionType>().ToList();

            var tests = new[]
            {
                typeof(BinaryExpressionTests),
                typeof(UnaryExpressionTests),
                typeof(MiscellaneousExpressionTests)
            }.SelectMany(
                t => t.GetMethods()
                    .Where(m => m.GetCustomAttribute<TestMethodAttribute>() != null &&
                                m.Name.StartsWith(t.Name.Substring(0, t.Name.Length - "Tests".Length)))
                    .Select(m => m.Name.Substring(t.Name.Length - "Tests".Length + 1)
                    )
            ).Select(e => (ExpressionType)Enum.Parse(typeof(ExpressionType), e, true)).ToList();
            
            var ignore = new[]
            {
                ExpressionType.DebugInfo,
                ExpressionType.Dynamic,
                ExpressionType.Extension,
                ExpressionType.Goto,
                ExpressionType.Label,
                ExpressionType.Loop,
                ExpressionType.Quote,
                ExpressionType.RuntimeVariables,
                ExpressionType.Throw
            };

            var missing = types.Except(tests).Where(e => !ignore.Contains(e)).OrderBy(e => e).ToList();

            if (missing.Count > 0)
                Assert.Fail($"{missing.Count} expression types do not have tests: " + string.Join(", ", missing));
        }
    }
}
