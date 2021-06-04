using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class ConversionTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_Integer() =>
            TestDouble("19", 19.0000, 19);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_DecimalBelowZero() =>
            TestDouble("0.1", 0.1000, 0.1);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_DecimalBelowZero() =>
            TestDouble("0,1", 0.1000, 0.1);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_DecimalAboveZero_TwoDecimalPlaces() =>
            TestDouble("60.95", 60.9500, 60.95);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_DecimalAboveZero_TwoDecimalPlaces() =>
            TestDouble("60,95", 60.9500, 60.95);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_DecimalAboveZero_ThreeDecimalPlaces() =>
            TestDouble("60.953", 60.9530, 60.953);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_DecimalAboveZero_ThreeDecimalPlaces() =>
            TestDouble("60,953", 60.9530, 60.953);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Thousand() =>
            TestDouble("3,726", 4000762036224.0000, 3726);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Thousand() =>
            TestDouble("3.726", 4000762036224.0000, 3726);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Thousand_WithDecimal_TwoDecimalPlaces() =>
            TestDouble("3,726.21", 4000762036224.2100, 3726.21);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Thousand_WithDecimal_TwoDecimalPlaces() =>
            TestDouble("3.726,21", 4000762036224.2100, 3726.21);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Thousand_WithDecimal_ThreeDecimalPlaces() =>
            TestDouble("3,726.213", 4000762036224.2130, 3726.213);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Thousand_WithDecimal_ThreeDecimalPlaces() =>
            TestDouble("3.726,213", 4000762036224.2130, 3726.213);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Million_WithDecimal() =>
            TestDouble("3,726,432.21", 4000762036224.2100, 3726432.21);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Million_WithDecimal() =>
            TestDouble("3.726.432,21", 4000762036224.2100, 3726432.21);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_SameInteger_DifferentDecimal() =>
            TestDouble("851,337.123", 851337.1429, 851337.123);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_SameInteger_DifferentDecimal() =>
            TestDouble("851.337,123", 851337.1429, 851337.123);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_SameInteger_NoDecimal() =>
            TestDouble("851,337", 851337.1429, 851337);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_SameInteger_NoDecimal() =>
            TestDouble("851.337", 851337.1429, 851337);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_RoundUp_ToInteger() =>
            TestDouble("16,237", 16236.8736, 16237);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_RoundUp_ToInteger() =>
            TestDouble("16.237", 16236.8736, 16237);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_RoundUp_StringHasMoreDecimalPlaces() =>
            TestDouble("1.666667", 1.6667, 1.666667);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_RoundUp_StringHasMoreDecimalPlaces() =>
            TestDouble("1,666667", 1.6667, 1.666667);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_RoundUp_RawHasMoreDecimalPlaces() =>
            TestDouble("1.6667", 1.666667, 1.6667);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_RoundUp_RawHasMoreDecimalPlaces() =>
            TestDouble("1,6667", 1.666667, 1.6667);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_RoundUp_ThousandMatchesRoundedDecimal() =>
            TestDouble("42,778", 44856456689.7778, 42778);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_RoundUp_ThousandMatchesRoundedDecimal() =>
            TestDouble("42.778", 44856456689.7778, 42778);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Bytes_WholeNumber() =>
            TestDouble("365,006", 382736842424.889, 365006);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Bytes_WholeNumber() =>
            TestDouble("365.006", 382736842424.889, 365006);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Bytes_Decimal() =>
            TestDouble("365,006.297", 382736842424.889, 365006.297);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Bytes_Decimal() =>
            TestDouble("365.006,297", 382736842424.889, 365006.297);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Bytes_RawDecimal_RoundedUp() =>
            TestDouble("1,042", 1066537.3092, 1042);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Bytes_RawDecimal_RoundedUp() =>
            TestDouble("1.042", 1066537.3092, 1042);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_Incomprehensible_Over1000() =>
            TestDouble("1,234", 772526.123, 1234);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_Incomprehensible_Over1000() =>
            TestDouble("1.234", 772526.123, 1234);

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_CloseRounding()
        {
            //Due to how .NET extracts the decimal point, .10975 could become .1097499999... and thus fail to round to 4 decimal places properly
            TestDouble("20,10975", 20.1098, 20.10975);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_CloseRounding()
        {
            //Due to how .NET extracts the decimal point, .10975 could become .1097499999... and thus fail to round to 4 decimal places properly
            TestDouble("20.10975", 20.1098, 20.10975);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_CloseRounding_AwayFromZero()
        {
            TestDouble("97,17305", 97.1731, 97.17305);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_CloseRounding_AwayFromZero()
        {
            TestDouble("97.17305", 97.1731, 97.17305);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_CloseRounding_CascadeRound()
        {
            TestDouble("2,826947", 2.8269, 2.826947);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_CloseRounding_CascadeRound()
        {
            TestDouble("2.826947", 2.8269, 2.826947);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_CloseRounding_SilentTruncate()
        {
            TestDouble("112,12165", 112.1216, 112.12165);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_CloseRounding_SilentTruncate()
        {
            TestDouble("112.12165", 112.1216, 112.12165);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_RawInteger_DecimalDisplay()
        {
            TestDouble("19.00001", 19, 19.00001);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_US_RawInteger_DecimalDisplay_Thousands()
        {
            //Will take multiple marks "easy path"
            TestDouble("1,019.00001", 1019, 1019.00001);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_RawInteger_DecimalDisplay()
        {
            TestDouble("19,00001", 19, 19.00001);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_EU_RawInteger_DecimalDisplay_Thousands()
        {
            //Will take multiple marks "easy path"
            TestDouble("1.019,00001", 1019, 1019.00001);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_ToDouble_RawDecimal_IntegerDisplay()
        {
            //We're only interested in converting the _display value_, so if the raw value is different, who cares!
            TestDouble("19", 19.00001, 19);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_AllCultures_NoMarks()
        {
            TestAllCultures(123);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_AllCultures_OneNumberMark_Thousands()
        {
            TestAllCultures(1234);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_AllCultures_OneNumberMark_Decimal()
        {
            TestAllCultures(123.456);
        }

        [UnitTest]
        [TestMethod]
        public void Convert_AllCultures_TwoNumberMarks()
        {
            TestAllCultures(1234.5678);
        }

        private void TestAllCultures(double number)
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            foreach (var culture in cultures)
            {
                var str = number.ToString("#,##0.####", culture);

                try
                {
                    TestDouble(str, number, number);
                }
                catch (Exception ex)
                {
                    throw new AssertFailedException($"Failed to convert number '{str}' in culture '{culture}': {ex.Message}", ex);
                }
            }
        }

        private void TestDouble(string s, double d, double expected)
        {
            var result = ConvertUtilities.ToDynamicDouble(s, d);

            Assert.AreEqual(expected, result);
        }
    }
}
