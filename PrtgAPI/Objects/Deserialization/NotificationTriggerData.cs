using System.Linq;
using System.Runtime.Serialization;
using PrtgAPI.Helpers;

namespace PrtgAPI.Objects.Deserialization
{
#pragma warning disable CS0649

    [DataContract]
    class NotificationTriggerData
    {
        [DataMember(Name = "supported")]
        private string[] supportedTypes;

        public TriggerType[] SupportedTypes => supportedTypes.Select(t => t.ToEnum<TriggerType>()).ToArray();

        [DataMember(Name = "data")]
        public NotificationTrigger[] Triggers { get; set; }

        [DataMember(Name = "readonly")]
        public bool ReadOnly { get; set; }
    }
#pragma warning restore CS0649
}
