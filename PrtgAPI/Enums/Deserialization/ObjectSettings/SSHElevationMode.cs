using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum SSHElevationMode
    {
        [XmlEnum("1")]
        RunAsUser,

        [XmlEnum("2")]
        RunAsAnotherWithPasswordViaSudo,

        [XmlEnum("4")] //This is not a bug
        RunAsAnotherWithoutPasswordViaSudo,

        [XmlEnum("3")] //This is not a bug
        RunAsAnotherViaSu
    }
}
