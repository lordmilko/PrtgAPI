using System.Runtime.Serialization;

namespace PrtgAPI.Targets
{
    [DataContract]
    class SensorTargetProgress
    {
        [DataMember(Name = "progress")]
        public int Percent { get; set; }

        [DataMember(Name = "targeturl")]
        public string TargetUrl { get; set; }
    }
}
