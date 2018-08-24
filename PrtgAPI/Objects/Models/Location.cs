using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Request;

namespace PrtgAPI
{
    [DataContract]
    internal class Location : IFormattableMultiple
    {
        [DataMember(Name = "formatted_address")]
        public string Address { get; set; }

        public double Latitude => Geometry.Location.Latitude;

        public double Longitude => Geometry.Location.Longitude;

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

        internal static Location Resolve(PrtgClient client, object value)
        {
            if (value == null)
                return new Location();

            List<Location> result = new List<Location>();

            for (int i = 0; i < 10; i++)
            {
                result = client.ResolveAddress(value.ToString());

                if (result.Any())
                    break;

#if !DEBUG
                Thread.Sleep(1000);
#endif
            }

            if (!result.Any())
                throw new PrtgRequestException($"Could not resolve '{value}' to an actual address");

            return result.First();
        }

        internal static async Task<Location> ResolveAsync(PrtgClient client, object value)
        {
            if (value == null)
                return new Location();

            List<Location> result = new List<Location>();

            for (int i = 0; i < 10; i++)
            {
                result = await client.ResolveAddressAsync(value.ToString()).ConfigureAwait(false);

                if (result.Any())
                    break;

#if !DEBUG
                await Task.Delay(1000).ConfigureAwait(false);
#endif
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
        internal Coordinates Location { get; set; }
    }

    /// <summary>
    /// Represents a geographic coordinate used to specify a physical location.
    /// </summary>
    [DataContract]
    public class Coordinates
    {
        /// <summary>
        /// The north/south position of the coordinate.
        /// </summary>
        [DataMember(Name = "lat")]
        public double Latitude { get; set; }

        /// <summary>
        /// The east/west position of the coordinate.
        /// </summary>
        [DataMember(Name = "lng")]
        public double Longitude { get; set; }

        /// <summary>
        /// The latitude and longitude of these coordinates in the form <see cref="Latitude"/>,<see cref="Longitude"/>.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Latitude},{Longitude}";
        }
    }
}
