using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum DependencyType
    {
        [XmlEnum("0")]
        Parent,

        [XmlEnum("1")]
        Object,

        [XmlEnum("2")]
        MasterObject //when the master object goes down all other objects under the device go down too
    }
}
