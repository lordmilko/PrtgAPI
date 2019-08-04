using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using PrtgAPI.Request;

namespace PrtgAPI
{
    [DataContract]
    [ExcludeFromCodeCoverage]
    class SensorTypeDescriptorInternal
    {
        [DataMember(Name = "sensortypes")]
        public List<SensorTypeDescriptor> Types { get; set; }
    }

    /// <summary>
    /// <para type="description">Describes a sensor type that can be applied under an object.</para>
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("Id = {Id}, Name = {Name}, Description = {Description}")]
    public class SensorTypeDescriptor
    {
        /// <summary>
        /// The internal identifier of the sensor type.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of the sensor type.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// A description of the sensor type.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /*
        //todo: all of these properties need to have double/triple fields to remove string.empty values
        //copy the technique used for serverstatus
            
        [DataMember(Name = "family")]
        public string[] Family { get; set; }

        [DataMember(Name = "help")]
        public string Help { get; set; }

        [DataMember(Name = "manuallink")]
        public string HelpLink { get; set; }

        [DataMember(Name = "needsprobe")]
        public bool NeedsProbe { get; set; }

        [DataMember(Name = "needslocalprobe")]
        public bool NeedsLocalProbe { get; set; }

        [DataMember(Name = "needsprobedevice")]
        public bool NeedsProbeDevice { get; set; }

        [DataMember(Name = "needsvm")]
        public bool NeedsVM { get; set; }

        [DataMember(Name = "needslinux")]
        public bool NeedsLinux { get; set; }

        [DataMember(Name = "needswindows")]
        public bool NeedsWindows { get; set; }

        [DataMember(Name = "dotnetversion")]
        public int DotNetVersion { get; set; }

        [DataMember(Name = "notincluster")]
        public bool NotInCluster { get; set; }

        [DataMember(Name = "notonpod")]
        public bool NotOnPod { get; set; }

        [DataMember(Name = "ipv6")]
        public bool IPv6 { get; set; }

        [DataMember(Name = "top10")]
        public int Top10 { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "resourceusage")]
        public string ResourceUsage { get; set; }

        [DataMember(Name = "linux")]
        public bool Linux { get; set; }

        [DataMember(Name = "metascan")]
        public string MetaScan { get; set; }

        [DataMember(Name = "templatesupport")]
        public bool TemplateSupport { get; set; }

        [DataMember(Name = "preselection")]
        public bool PreSelection { get; set; }*/

        private string preSelectionList;

        [DataMember(Name = "preselectionlist")]
        internal string PreSelectionList
        {
            get { return preSelectionList; }
            set
            {
                preSelectionList = WebUtility.UrlDecode(value);

                var lists = HtmlParser.Default.GetDropDownList(preSelectionList).SingleOrDefault();

                if(lists != null)
                    QueryTargets = lists.Options.Where(o => o.Value != string.Empty).Select(o => new SensorQueryTarget(o.Value)).ToList();
            }
        }

        /// <summary>
        /// The query targets that can be specified when querying the resources required to create a new sensor of this type.
        /// </summary>
        public List<SensorQueryTarget> QueryTargets { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Name} ({Id})";
        }
    }
}
