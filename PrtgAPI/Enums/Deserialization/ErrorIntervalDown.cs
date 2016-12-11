using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI.Enums.Deserialization
{
    public enum ErrorIntervalDown
    {
        [XmlEnum("0")]
        DownImmediately,

        [XmlEnum("1")]
        OneWarningThenDown,

        [XmlEnum("2")]
        TwoWarningsThenDown,

        [XmlEnum("3")]
        ThreeWarningsThenDown,

        [XmlEnum("4")]
        FourWarningsThenDown,

        [XmlEnum("5")]
        FiveWarningsThenDown
    }
}
