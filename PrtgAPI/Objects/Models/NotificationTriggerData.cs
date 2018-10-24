using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using PrtgAPI.Utilities;

namespace PrtgAPI
{
#pragma warning disable CS0649

    [DataContract]
    class NotificationTriggerData
    {
        [DataMember(Name = "supported")]
        private string[] supportedTypes;

        public TriggerType[] SupportedTypes => supportedTypes.Select(t => t.ToEnum<TriggerType>()).ToArray();

        [ExcludeFromCodeCoverage]
        [DataMember(Name = "data")]
        public NotificationTrigger[] Triggers { get; set; }

        [ExcludeFromCodeCoverage]
        [DataMember(Name = "readonly")]
        public bool ReadOnly { get; set; }
    }
#pragma warning restore CS0649
}
