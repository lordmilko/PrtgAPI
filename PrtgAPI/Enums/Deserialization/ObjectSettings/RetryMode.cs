using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum RetryMode
    {
        [XmlEnum("1")]
        Retry,

        [XmlEnum("0")]
        DoNotRetry
    }
}
