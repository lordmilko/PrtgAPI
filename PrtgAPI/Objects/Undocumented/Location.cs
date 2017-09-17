using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI
{
    [DataContract]
    internal class Location : IFormattableMultiple
    {
        [DataMember(Name = "formatted_address")]
        public string Address { get; set; }

        public string Latitude => Geometry.Location.Latitude;

        public string Longitude => Geometry.Location.Longitude;

        [DataMember(Name = "geometry")]
        private GeoResultGeometry Geometry { get; set; }

        public override string ToString()
        {
            return Address;
        }

        string IFormattable.GetSerializedFormat()
        {
            return Address;
        }

        string[] IFormattableMultiple.GetSerializedFormats()
        {
            return new[]
            {
                Address,
                $"{Longitude},{Latitude}"
            };
        }

        internal static Location Resolve(PrtgClient client, PrtgObject @object, object value)
        {
            var result = client.ResolveAddress(value.ToString());

            if (!result.Any())
                throw new PrtgRequestException($"Could not resolve '{value}' to an actual address");

            return result.First();
        }
    }

    [DataContract]
    internal class GeoResult
    {
        [DataMember(Name = "results")]
        internal Location[] Results { get; set; }
    }

    [DataContract]
    internal class GeoResultGeometry
    {
        [DataMember(Name = "location")]
        internal GeoResultLocation Location { get; set; }
    }

    [DataContract]
    internal class GeoResultLocation
    {
        [DataMember(Name = "lat")]
        internal string Latitude { get; set; }

        [DataMember(Name = "lng")]
        internal string Longitude { get; set; }
    }
}
