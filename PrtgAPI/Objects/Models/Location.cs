using System.Runtime.Serialization;
using PrtgAPI.Request;

namespace PrtgAPI
{
    internal class GpsLocation : Location
    {
        public override double Latitude { get; }

        public override double Longitude { get; }

        internal GpsLocation(double latitude, double longitude)
        {
            Address = $"{latitude}, {longitude}";
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    [DataContract]
    internal class Location : IMultipleSerializable
    {
        public virtual string Address { get; set; }

        public virtual double Latitude { get; }

        public virtual double Longitude { get; }

        public override string ToString()
        {
            return Address;
        }

        string Request.ISerializable.GetSerializedFormat()
        {
            return Address;
        }

        string[] IMultipleSerializable.GetSerializedFormats()
        {
            if (Address == null)
                return new string[] { null, null };

            return new[]
            {
                Address,
                $"{Longitude},{Latitude}"
            };
        }
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
