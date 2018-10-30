using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    [DataContract]
    internal class HereOuterResponse
    {
        [DataMember(Name = "Response")]
        internal HereInnerResponse Response { get; set; }
    }

    [DataContract]
    internal class HereInnerResponse
    {
        [DataMember(Name = "View")]
        internal List<HereView> View { get; set; }
    }

    [DataContract]
    internal class HereView
    {
        [DataMember(Name = "Result")]
        internal List<HereResult> Results { get; set; }
    }

    [DataContract]
    internal class HereResult
    {
        [DataMember(Name = "Location")]
        internal HereLocation Location { get; set; }
    }

    [DataContract]
    internal class HereLocation : Location
    {
        [DataMember(Name = "DisplayPosition")]
        internal HereDisplayPosition DisplayPosition { get; set; }

        [DataMember(Name = "Address")]
        internal HereAddressInternal AddressInternal { get; set; }

        public override string Address => AddressInternal.Label;

        public override double Latitude => DisplayPosition.Latitude;

        public override double Longitude => DisplayPosition.Longitude;
    }

    [DataContract]
    internal class HereDisplayPosition
    {
        [DataMember(Name = "Latitude")]
        internal double Latitude { get; set; }

        [DataMember(Name = "Longitude")]
        internal double Longitude { get; set; }
    }

    [DataContract]
    internal class HereAddressInternal
    {
        [DataMember(Name = "Label")]
        internal string Label { get; set; }
    }
}
