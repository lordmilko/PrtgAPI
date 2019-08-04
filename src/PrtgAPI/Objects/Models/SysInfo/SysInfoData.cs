using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    [DataContract]
    internal class SysInfoData<T> where T : IDeviceInfo
    {
        [DataMember(Name = "prtg-version")]
        public string Version { get; set; }

        [DataMember(Name = "treesize")]
        public string TreeSize { get; set; }

        [DataMember(Name = "sysinfo")]
        public List<T> Items { get; set; }
    }
}
