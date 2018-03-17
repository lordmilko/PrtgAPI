using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    [DataContract]
    [ExcludeFromCodeCoverage]
    class SensorTypeDescriptorInternal
    {
        [DataMember(Name = "sensortypes")]
        public List<SensorTypeDescriptor> Types { get; set; }
    }

    /// <summary>
    /// Describes a sensor type that can be applied under an object.
    /// </summary>
    [DataContract]
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("Id = {Id}, Name = {Name}, Description = {Description}")]
    public class SensorTypeDescriptor
    {
        /// <summary>
        /// The internal identifier of the sensor type.
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
