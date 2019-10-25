using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace PrtgAPI.Utilities
{
    internal class ConvertUtilities
    {
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
