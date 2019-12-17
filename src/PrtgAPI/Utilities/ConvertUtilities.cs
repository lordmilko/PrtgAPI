using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace PrtgAPI.Utilities
{
    internal class ConvertUtilities
    {
        /// <summary>
        /// Converts a string value to a <see cref="double"/> by dynamically calculating the specified number format.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <param name="raw">The raw numeric representation of the string.</param>
        /// <returns>A double that represents the specified string.</returns>
        internal static double ToDynamicDouble(string str, double? raw)
        {
            str = str.Trim();

            var hasDouble = raw != null;

            var numPeriods = str.Count(c => c == '.');
            var numCommas = str.Count(c => c == ',');

            var multipleMarks = numPeriods > 0 && numCommas > 0;

            //If there is no punctuation it's a regular integer value, so just convert it
            if (numPeriods == 0 && numCommas == 0)
                return ToPeriodDecimal(str);

            //As you can't have two decimal points, having two of any punctuation mark
            //indicates that mark must be the thousands operator, so convert using the appropriate culture
            if (numCommas > 1)
                return ToPeriodDecimal(str);

            if (numPeriods > 1)
                return ToCommaDecimal(str);

            //If the number is between 0 and 1 we can just check what the number starts with
            if (str.StartsWith("0."))
                return ToPeriodDecimal(str);

            if (str.StartsWith("0,"))
                return ToCommaDecimal(str);

            //If we have both punctuation marks, whichever comes first is the thousands separator
            if (multipleMarks)
            {
                var strFirstMarkIndex = str.IndexOfAny(new[] {'.', ','});

                if (str[strFirstMarkIndex] == '.')
                    return ToCommaDecimal(str);
                else
                    return ToPeriodDecimal(str);
            }

            //The value is either a whole number >= 1000 or a decimal number < 1000
            //If there's fewer than 3 digits after the last punctuation mark, that mark must be the period separator

            var strLastMarkIndex = str.LastIndexOfAny(new[] {'.', ','});
            var strLastMark = str[strLastMarkIndex];

            //We have fewer than 3 decimal places
            if (strLastMark == '.' && strLastMarkIndex >= str.Length - 3)
                return ToPeriodDecimal(str);

            if (strLastMark == ',' && strLastMarkIndex >= str.Length - 3)
                return ToCommaDecimal(str);

            if (hasDouble)
            {
                //Compare the decimal component of the raw number

                var rawStr = raw.Value.ToString(CultureInfo.InvariantCulture);
                var rawLastMarkIndex = rawStr.LastIndexOfAny(new[] {'.', ','});

                if (rawLastMarkIndex == -1)
                {
                    //All zeroes. Whatever punctuation we have is for thousands
                    if (numCommas > 0)
                        return ToPeriodDecimal(str);
                    else
                        return ToCommaDecimal(str);
                }
                else
                {
                    var numDecStr = rawStr.Substring(rawLastMarkIndex + 1).TrimEnd('0'); //Trim trailing decimal zeroes
                    var strDecStr = str.Substring(strLastMarkIndex + 1);

                    if (numDecStr == strDecStr)
                    {
                        //The character at lastIndex is the decimal separator
                        if (strLastMark == '.')
                            return ToPeriodDecimal(str);
                        else
                            return ToCommaDecimal(str);
                    }
                    else
                    {
                        //Maybe the number of decimal places was rounded up is a match (e.g. 1.67 vs 1.6667).
                        //If the raw number is bigger than 1000, we'll catch it in the catch all at the end.

                        if (strLastMark == '.')
                        {
                            double val;

                            if (raw < 1000 && TryRoundDecimal(str, raw.Value, ToPeriodDecimal, numDecStr, strDecStr, out val))
                                return val;
                        }
                        else
                        {
                            double val;

                            if (raw < 1000 & TryRoundDecimal(str, raw.Value, ToCommaDecimal, numDecStr, strDecStr, out val))
                                return val;
                        }
                    }
                }

                //Compare the integral part of the raw number

                var numIntStr = rawStr.Substring(0, rawLastMarkIndex).TrimStart('0');
                var strIntStr = str.TrimStart('0');

                //If we get to this stage we know we don't have multiple marks; if we did, we would have
                //shortcut out by inspecting the first mark above. Therefore, we most likely have a display
                //value that doesn't have a decimal and a raw value that does

                var strClean = strIntStr.Replace(".", string.Empty).Replace(",", string.Empty);

                if (strLastMark == ',' && numIntStr == strClean)
                    return ToPeriodDecimal(str);

                if (strLastMark == '.' && numIntStr == strClean)
                    return ToCommaDecimal(str);

                //Maybe the display value is the numeric value rounded up to an integer

                var numRoundedStr = Math.Round(raw.Value).ToString(CultureInfo.InvariantCulture);

                if (strLastMark == ',' && numRoundedStr == strClean)
                    return ToPeriodDecimal(str);

                if (strLastMark == '.' && numRoundedStr == strClean)
                    return ToCommaDecimal(str);

                //Maybe the display value is in bytes
                var divided = raw.Value;
                var strDecStr1 = str.Substring(strLastMarkIndex + 1);

                while (divided > 1)
                {
                    divided = divided / 1024;

                    if (strClean == ((int) divided).ToString())
                    {
                        //All we have is the thousands separator
                        if (strLastMark == ',')
                            return ToPeriodDecimal(str);
                        else
                            return ToCommaDecimal(str);
                    }
                    else
                    {
                        Func<string, double> func;

                        if (strLastMark == ',')
                            func = ToPeriodDecimal;
                        else
                            func = ToCommaDecimal;

                        if (strClean == Math.Round(divided).ToString())
                            return func(str);
                    }
                }

                //Maybe it's incomprehensible. If the raw number is >= 1000 though, we know it must represent a thousand

                if (raw >= 1000)
                {
                    if (strLastMark == ',')
                        return ToPeriodDecimal(str);
                    else
                        return ToCommaDecimal(str);
                }
            }

            throw new NotImplementedException($"Don't know how to convert double value '{str}' ({raw}).");
        }

        private static bool TryRoundDecimal(string str, double raw, Func<string, double> toDecimal, string numDecStr, string strDecStr, out double result)
        {
            //Either the string or raw version could have been rounded

            var strDouble = toDecimal(str);

            var rawDecimalOnly = raw - Math.Floor(raw);
            var strDecimalOnly = strDouble - Math.Floor(strDouble);

            //Find whoever has the fewest decimal places, round them to be the same and then compare them
            var decimalPlaces = Math.Min(numDecStr.Length, strDecStr.Length);

            var rawDecimalOnlyRounded = Math.Round(rawDecimalOnly, decimalPlaces);
            var strDecimalOnlyRounded = Math.Round(strDecimalOnly, decimalPlaces);

            if (rawDecimalOnlyRounded == strDecimalOnlyRounded)
            {
                result = strDouble;
                return true;
            }

            result = default(double);
            return false;
        }

        private static double ToPeriodDecimal(string s)
        {
            return double.Parse(s, NumberFormatInfo.InvariantInfo);
        }

        private static double ToCommaDecimal(string s)
        {
            return double.Parse(s, NumberFormatInfo.GetInstance(CultureInfo.GetCultureInfo("de-DE")));
        }

        /// <summary>
        /// Converts the value of a potentially non-invariant string to a double-precision floating-point number.
        /// </summary>
        /// <param name="s">The value to convert.</param>
        /// <returns>The string representation of the specified value.</returns>
        [ExcludeFromCodeCoverage]
        internal static double ToDouble(string s)
        {
            //todo: with both this and the xmlparserbaseone, what if they BOTH succeed...that indicates maybe we had 0,1

            s = s.Trim();

            if (s == "-INF")
                return double.NegativeInfinity;
            if (s == "INF")
                return double.PositiveInfinity;

            //Note the lack of NumberStyles.AllowThousands (included by default in double.Parse() with no number style specified). This will prevent
            //values from 0,1 being converted into 1 on a non-EU culture and appearing to be "successful" when they shouldn't be.
            var numberStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

            double dVal;

            //XML values should always be InvariantCulture. If value was scraped from HTML, value could use a comma for decimal points
            //(e.g. if European culture). If we can't convert with InvariantCulture, first let's try and convert with the user's native culture
            if (!double.TryParse(s, numberStyle, NumberFormatInfo.InvariantInfo, out dVal) && !double.TryParse(s, numberStyle, NumberFormatInfo.CurrentInfo, out dVal))
            {
                //If neither InvariantCulture nor CurrentCulture worked, this indicates there's most likely a mismatch between the client and server cultures (e.g. server was
                //, but client is .). Let's try and make our value InvariantCulture compliant and hope for the best
                dVal = double.Parse(s.Replace(",", "."), numberStyle, NumberFormatInfo.InvariantInfo);
            }

            return dVal;
        }
    }
}
