using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// <para type="description">Base class for all PRTG objects.</para>
    /// </summary>
    public class PrtgObject : IFormattable
    {
        // ################################## All Object Tables ##################################

        //prtg's documentation says these belong under ObjectTable, however i believe they may belong under PrtgObject

        /// <summary>
        /// ID number used to uniquely identify this object within PRTG.
        /// </summary>
        [XmlElement("objid")]
        [PropertyParameter(nameof(Property.Id))]
        public int Id { get; set; }

        /// <summary>
        /// Name of this object.
        /// </summary>
        [XmlElement("name")]
        [PropertyParameter(nameof(Property.Name))]
        public string Name { get; set; }

        private string raw;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgObject"/> class.
        /// </summary>
        public PrtgObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgObject"/> class with the raw representation of an object.
        /// </summary>
        /// <param name="raw">The raw representation of the object, containing the object's ID and Name.</param>
        /// <param name="callback">A callback used to initialize additional properties of the object.</param>
        internal PrtgObject(string raw, Action<string[]> callback = null)
        {
            if (raw == null)
                throw new ArgumentNullException(nameof(raw));

            if(raw == "-1")
            {
                Id = -1;
                Name = "None";
            }
            else
            {
                this.raw = raw;
                var components = raw.Split('|');
                Id = Convert.ToInt32(components[0]);
                Name = components[1];
            }
        }

        string IFormattable.GetSerializedFormat()
        {
            return raw ?? ToString();
        }
    }
}
