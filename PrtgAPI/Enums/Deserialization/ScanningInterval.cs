using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum ScanningInterval
    {
        [XmlEnum("30|30 seconds")]
        ThirtySeconds,

        [XmlEnum("60|60 seconds")]
        SixtySeconds,

        [XmlEnum("300|5 minutes")]
        FiveMinutes,

        [XmlEnum("600|10 minutes")]
        TenMinutes,

        [XmlEnum("900|15 minutes")]
        FifteenMinutes,

        [XmlEnum("1800|30 minutes")]
        ThirtyMinutes,

        [XmlEnum("3600|1 hour")]
        OneHour,

        [XmlEnum("14400|4 hours")]
        FourHours,

        [XmlEnum("21600|6 hours")]
        SixHours,

        [XmlEnum("43200|12 hours")]
        TwelveHours,

        [XmlEnum("86400|24 hours")]
        TwentyFourHours,

        //todo: what if a custom scanninginterval has been specified
    }
}
