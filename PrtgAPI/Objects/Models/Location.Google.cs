using System.Runtime.Serialization;

namespace PrtgAPI
{
#pragma warning disable 0649 //Field is never assigned to, and will alwayas have its default value null
    [DataContract]
    internal class GoogleGeoResult
    {
        [DataMember(Name = "error_message")]
        internal string ErrorMessage { get; set; }

        [DataMember(Name = "results")]
        internal GoogleLocation[] Results { get; set; }

        [DataMember(Name = "status")]
        internal string Status { get; set; }
    }

    [DataContract]
    internal class GoogleLocation : Location
    {
        [DataMember(Name = "formatted_address")]
        private string formattedAddress;

        public override string Address => formattedAddress;

        public override double Latitude => Geometry.Location.Latitude;

        public override double Longitude => Geometry.Location.Longitude;

        [DataMember(Name = "geometry")]
        private GoogleGeoResultGeometry Geometry { get; set; }
    }

    [DataContract]
    internal class GoogleGeoResultGeometry
    {
        [DataMember(Name = "location")]
        internal Coordinates Location { get; set; }
    }
#pragma warning restore 0649
}
