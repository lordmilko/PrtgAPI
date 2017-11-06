using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    [DataContract]
    class SensorTypeDescriptorInternal
    {
        [DataMember(Name = "sensortypes")]
        public List<SensorTypeDescriptor> Types { get; set; }
    }

    /// <summary>
    /// Describes a sensor type that can be applied under an object.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("Id = {Id}, Name = {Name}, Description = {Description}")]
    public class SensorTypeDescriptor
    {
        /// <summary>
        /// The ID of the object the sensor will be created under.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of the sensor type.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// A description of the sensor type.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}
