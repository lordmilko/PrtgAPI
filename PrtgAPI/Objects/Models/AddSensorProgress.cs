using System.Runtime.Serialization;

namespace PrtgAPI
{
    [DataContract]
    class AddSensorProgress
    {
        [DataMember(Name = "progress")]
        public int Percent { get; set; }

        [DataMember(Name = "targeturl")]
        public string TargetUrl { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }
    }
}
