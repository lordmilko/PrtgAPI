using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

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
            if (Address == null)
                return new string[] {null, null};

            return new[]
            {
                Address,
                $"{Longitude},{Latitude}"
            };
        }

        internal static Location Resolve(PrtgClient client, int objectId, object value)
        {
            if (value == null)
                return new Location();

            List<Location> result = new List<Location>();

            for (int i = 0; i < 10; i++)
            {
                result = client.ResolveAddress(value.ToString());

                if (result.Any())
                    break;

                Thread.Sleep(1000);
            }

            if (!result.Any())
                throw new PrtgRequestException($"Could not resolve '{value}' to an actual address");

            return result.First();
        }

        internal static async Task<Location> ResolveAsync(PrtgClient client, int objectId, object value)
        {
            if (value == null)
                return new Location();

            List<Location> result = new List<Location>();

            for (int i = 0; i < 10; i++)
            {
                result = await client.ResolveAddressAsync(value.ToString());

                if (result.Any())
                    break;

                Thread.Sleep(1000);
            }

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
