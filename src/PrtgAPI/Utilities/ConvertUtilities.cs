using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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

            var groups = Regex.Replace(str, "\\d", string.Empty).ToCharArray().GroupBy(c => c).ToArray();

            if (groups.Length > 2)
                throw new ArgumentException($"Cannot parse display value '{str}': value should have at most two numeric group separators, however {groups.Length} were found.");

            char? markA = null;
            char? markB = null;

            char[] chars;

            var numA = 0;
            var numB = 0;

            if (groups.Length > 0)
            {
                markA = groups[0].Key;
                numA = groups[0].Count();

                if (groups.Length > 1)
                {
                    markB = groups[1].Key;
                    numB = groups[1].Count();

                    chars = new[] {markA.Value, markB.Value};
                }
                else
                    chars = new[] {markA.Value};
            }
            else
                chars = new char[0];

            var multipleMarks = numA > 0 && numB > 0;

            //If there is no punctuation it's a regular integer value, so just convert it
            if (numA == 0 && numB == 0)
                return ToDecimal(str, thousands: markA, @decimal: markB);

            //As you can't have two decimal points, having two of any punctuation mark
            //indicates that mark must be the thousands operator, so convert using the appropriate culture
            if (numA > 1)
                return ToDecimal(str, thousands: markA, @decimal: markB);

            //If we have more than 1 thousands separators, it would occurred before any decimal separators; as such, we would expect this code path
            //never gets hit
            if (numB > 1)
                return ToDecimal(str, thousands: markB, @decimal: markA);

            if (str.StartsWith("0") && str.Length > 1)
            {
                //If the number is between 0 and 1 we can just check what the number starts with
                if (str[1] == markA)
                    return ToDecimal(str, thousands: markB, @decimal: markA);

                //Implicitly if the number starts with "0" there is _only_ markA
                if (str[1] == markB)
                    return ToDecimal(str, thousands: markA, @decimal: markB);
            }

            //If we have both punctuation marks, whichever comes first is the thousands separator
            if (multipleMarks)
            {
                var strFirstMarkIndex = str.IndexOfAny(chars);

                //markA by definition always comes first, so we would expect the else pathway here never gets hit
                if (str[strFirstMarkIndex] == markA)
                    return ToDecimal(str, thousands: markA, @decimal: markB);
                else
                    return ToDecimal(str, thousands: markB, @decimal: markA);
            }

            //The value is either a whole number >= 1000 or a decimal number < 1000
            //If there's fewer than 3 digits after the last punctuation mark, that mark must be the period separator

            var strLastMarkIndex = str.LastIndexOfAny(chars);
            var strLastMark = str[strLastMarkIndex];

            //We have fewer than 3 decimal places
            if (strLastMark == markA && strLastMarkIndex >= str.Length - 3)
                return ToDecimal(str, thousands: markB, @decimal: markA);

            if (strLastMark == markB && strLastMarkIndex >= str.Length - 3)
                return ToDecimal(str, thousands: markA, @decimal: markB);

            if (hasDouble)
            {
                //Compare the decimal component of the raw number

                //We know we have _some kind of numeric separator_ (after all, if we didn't we would have already returned a simple value above)
                var rawStr = raw.Value.ToString("#,##0.####", CultureInfo.InvariantCulture);
                var rawLastMarkIndex = rawStr.LastIndexOfAny(new[] {',', '.'});

                if (rawLastMarkIndex == -1)
                {
                    //The raw value has no decimal, so we know that the punctuation mark in the
                    //display value must be the decimal point! We also know that numA or numB has exactly
                    //1 punctuation mark (since if there were two, we would have taken multiple mark fast path above)
                    if (numA == 0)
                        return ToDecimal(str, thousands: markA, @decimal: markB); //numB MUST have exactly 1 mark
                    else
                        return ToDecimal(str, thousands: markB, @decimal: markA); //numA MUST have exactly 1 mark
                }

                if (rawStr[rawLastMarkIndex] == '.')
                {
                    //The raw value definitely has a decimal component on it

                    var numDecStr = rawStr.Substring(rawLastMarkIndex + 1).TrimEnd('0'); //Trim trailing decimal zeroes
                    var strDecStr = str.Substring(strLastMarkIndex + 1);

                    if (numDecStr == strDecStr)
                    {
                        //The character at lastIndex is the decimal separator

                        if (strLastMark == markA)
                            return ToDecimal(str, thousands: markB, @decimal: markA);
                        else
                            return ToDecimal(str, thousands: markA, @decimal: markB);
                    }
                    else
                    {
                        //Maybe the number of decimal places was rounded up is a match (e.g. 1.67 vs 1.6667).
                        //If the raw number is bigger than 1000, we'll catch it in the catch all at the end.

                        char? thousands;
                        char? @decimal;

                        if (strLastMark == markA)
                        {
                            thousands = markB;
                            @decimal = markA;
                        }
                        else
                        {
                            thousands = markA;
                            @decimal = markB;
                        }

                        double val;

                        if (raw < 1000 && TryRoundDecimal(str, raw.Value, s => ToDecimal(s, thousands: thousands, @decimal: @decimal), numDecStr, strDecStr, out val))
                            return val;
                    }
                }

                //Compare the integral part of the raw number

                var numIntStr = rawStr.Substring(0, rawLastMarkIndex).TrimStart('0');
                var strIntStr = str.TrimStart('0');

                //If we get to this stage we know we don't have multiple marks; if we did, we would have
                //shortcut out by inspecting the first mark above. Therefore, we most likely have a display
                //value that doesn't have a decimal and a raw value that does

                var strClean = strIntStr;

                if (markA != null)
                    strClean = strClean.Replace(markA.ToString(), string.Empty);

                if (markB != null)
                    strClean = strClean.Replace(markB.ToString(), string.Empty);

                if (strLastMark == markA && numIntStr == strClean)
                    return ToDecimal(str, thousands: markA, @decimal: markB);

                if (strLastMark == markB && numIntStr == strClean)
                    return ToDecimal(str, thousands: markB, @decimal: markA);

                //Maybe the display value is the numeric value rounded up to an integer

                var numRoundedStr = Math.Round(raw.Value, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);

                if (strLastMark == markA && numRoundedStr == strClean)
                    return ToDecimal(str, thousands: markA, @decimal: markB);

                if (strLastMark == markB && numRoundedStr == strClean)
                    return ToDecimal(str, thousands: markB, @decimal: markA);

                //Maybe the display value is in bytes
                var divided = raw.Value;
                var strDecStr1 = str.Substring(strLastMarkIndex + 1);

                while (divided > 1)
                {
                    divided = divided / 1024;

                    if (strClean == ((int) divided).ToString())
                    {
                        //All we have is the thousands separator
                        if (strLastMark == markA)
                            return ToDecimal(str, thousands: markA, @decimal: markB);
                        else
                            return ToDecimal(str, thousands: markB, @decimal: markA);
                    }
                    else
                    {
                        Func<string, double> func;

                        if (strLastMark == markA)
                            func = s => ToDecimal(s, thousands: markA, @decimal: markB);
                        else
                            func = s => ToDecimal(s, thousands: markB, @decimal: markA);

                        if (strClean == Math.Round(divided, MidpointRounding.AwayFromZero).ToString())
                            return func(str);
                    }
                }

                //Maybe it's incomprehensible. If the raw number is >= 1000 though, we know it must represent a thousand

                if (raw >= 1000)
                {
                    if (strLastMark == markA)
                        return ToDecimal(str, thousands: markA, @decimal: markB);
                    else
                        return ToDecimal(str, thousands: markB, @decimal: markA);
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

            //Extracting the 10975 off of 20.10975 may have had the effect of turning the number into
            //the value 0.1097499999..; as such, if we were to round this to four decimal places to compare with
            //.1098, we would round _down_; as such; to make sure we have a fair comparison, we round 1 character at a time

            //Maximum number of decinal places Math.Round will allow you to specify
            var maxDecinalPlaces = 15;

            var rawDecimalOnlyRounded = rawDecimalOnly;
            var strDecimalOnlyRounded = strDecimalOnly;

            var strDecimalImmediateRounded = Math.Round(strDecimalOnlyRounded, decimalPlaces, MidpointRounding.AwayFromZero);

            var strDecimalTruncateStr = strDecimalOnlyRounded.ToString();
            var strDecimalPlaces = strDecimalTruncateStr.Substring(2);
            var strDecimalTruncateStrResult = strDecimalPlaces.Substring(0, Math.Min(decimalPlaces, strDecimalPlaces.Length));
            var strDecimalTruncated = Convert.ToDouble("0." + strDecimalTruncateStrResult, CultureInfo.InvariantCulture);

            for (var i = maxDecinalPlaces; i >= decimalPlaces; i--)
                rawDecimalOnlyRounded = Math.Round(rawDecimalOnlyRounded, i, MidpointRounding.AwayFromZero);

            for (var i = maxDecinalPlaces; i >= decimalPlaces; i--)
                strDecimalOnlyRounded = Math.Round(strDecimalOnlyRounded, i, MidpointRounding.AwayFromZero);

            if (rawDecimalOnlyRounded == strDecimalOnlyRounded || rawDecimalOnlyRounded == strDecimalImmediateRounded || rawDecimalOnlyRounded == strDecimalTruncated)
            {
                result = strDouble;
                return true;
            }

            result = default(double);
            return false;
        }

        private static double ToDecimal(string s, char? thousands, char? @decimal)
        {
            bool needsFixup;

            var culture = GetCulture(thousands, @decimal, out needsFixup);

            if (needsFixup)
            {
                if (thousands == null || @decimal == null)
                {
                    if (thousands == null)
                    {
                        if (@decimal != null)
                            s = s.Replace(@decimal.Value, '.');
                    }
                    else
                        s = s.Replace(thousands.Value, ',');
                }
                else
                {
                    var arr = s.ToCharArray();

                    for (var i = 0; i < arr.Length; i++)
                    {
                        if (arr[i] == thousands.Value)
                            arr[i] = ',';
                        else if (arr[i] == @decimal.Value)
                            arr[i] = '.';
                    }

                    s = new string(arr);
                }
            }

            return double.Parse(s, NumberFormatInfo.GetInstance(culture));
        }

        private static CultureInfo GetCulture(char? thousands, char? @decimal, out bool needsFixup)
        {
            if ((thousands == ',' && @decimal == '.') || (thousands == ',' && @decimal == null) || (@decimal == '.' && thousands == null))
            {
                needsFixup = false;
                return CultureInfo.InvariantCulture;
            }

            if ((thousands == '.' && @decimal == ',') || (thousands == '.' && @decimal == null) || (@decimal == ',' && thousands == null))
            {
                needsFixup = false;
                return CultureInfo.GetCultureInfo("de-DE");
            }

            //It's something weird; let's just manually turn it into an invariant string
            needsFixup = true;
            return CultureInfo.InvariantCulture;
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
