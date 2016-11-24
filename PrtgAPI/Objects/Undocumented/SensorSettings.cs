using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PrtgAPI.Objects.Undocumented
{
    public class SensorSettings : ObjectSettings
    {
        internal static XElement GetXml(string response, int sensorId)
        {
            var basicMatchRegex = "<input.+?name=\".*?\".+?value=\".*?\".*?>";
            var nameRegex = "(.+?name=\")(.+?)(_*\".+)"; //we might want to leave the underscores afterall

            return GetXmlInternal(response, sensorId, basicMatchRegex, nameRegex, null);
        }

        [XmlElement("injected_name")]
        public string Name { get; set; }

        [XmlElement("injected_parenttags")]
        public string ParentTags { get; set; }

        [XmlElement("injected_tags")]
        public string Tags { get; set; }

        [XmlElement("injected_writeresult")]
        public string WriteResult { get; set; }

        [XmlElement("injected_wmialternative")]
        public string WmiAlternative { get; set; }

        [XmlElement("injected_stack")]
        public string Stack { get; set; }

        [XmlElement("injected_stackunit")]
        public string StackUnit { get; set; }

        [XmlElement("injected_intervalgroup")]
        public string IntervalGroup { get; set; }

        [XmlElement("injected_scheduledependency")]
        public string ScheduleDependency { get; set; }

        [XmlElement("injected_maintenable")]
        public string MaintEnable {get; set; }

        [XmlElement("injected_maintstart")]
        public string MaintStart {get; set; }

        [XmlElement("injected_maintend")]
        public string MaintEnd {get; set; }

        [XmlElement("injected_dependencytype")]
        public string DependencyType {get; set; }

        [XmlElement("injected_depdelay")]
        public string DepDelay { get; set; }

        [XmlElement("injected_accessgroup")]
        public string AccessGroup { get; set; }

        [XmlElement("injected_unitconfiggroup")]
        public string UnitConfigGroup { get; set; }

        [XmlElement("injected_priority")]
        public string Priority { get; set; }

        [XmlElement("injected_primarychannel")]
        public string PrimaryChannel { get; set; }

        [XmlElement("injected_interval")]
        public string Interval { get; set; }

        [XmlElement("injected_errorintervalsdown")]
        public string ErrorIntervalsDown { get; set; }

        [XmlElement("injected_schedule")]
        public string Schedule { get; set; }

        [XmlElement("injected_accessrights_201")]
        public string AccessRights { get; set; }

        [XmlElement("injected_unitconfig__oukBytesMemory_volume")]
        public string UnitConfig__OUKBytesMemory_Volume { get; set; }





}
}
