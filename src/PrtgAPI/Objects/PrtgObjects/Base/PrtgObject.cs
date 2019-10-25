using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Request;
using PrtgAPI.Tree;
using PrtgAPI.Utilities;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Represents a uniquely identifiable object within PRTG.</para>
    /// </summary>
    [AlternateDescription("Object")]
    public class PrtgObject : IPrtgObject, ITableObject, ITreeValue, ISerializable
    {
        // ################################## All Object Tables ##################################

        //prtg's documentation says these belong under ObjectTable, however i believe they may belong under PrtgObject

        int? ITreeValue.Id => Id;

        /// <summary>
        /// Unique identifier of this object within PRTG.
        /// </summary>
        [XmlElement("objid")]
        [PropertyParameter(Property.Id)]
        public int Id { get; set; }

        private int parentId;

        /// <summary>
        /// ID of this object's parent.
        /// </summary>
        [XmlElement("parentid")]
        [PropertyParameter(Property.ParentId)]
        public int ParentId
        {
            get { return Lazy(() => parentId); }
            set { parentId = value; }
        }

        /// <summary>
        /// Name of this object.
        /// </summary>
        [XmlElement("name")]
        [PropertyParameter(Property.Name)]
        public string Name { get; set; }

        private string[] tags;

        /// <summary>
        /// Tags contained on this object.
        /// </summary>
        [XmlElement("tags")]
        [XmlElement("injected_tags")]
        [StandardSplittableString]
        [PropertyParameter(Property.Tags)]
        public string[] Tags
        {
            get { return Lazy(() => tags); }
            set { tags = value; }
        }

        private string displayType;

        /// <summary>
        /// The display type of this object. Certain objects may simply report their <see cref="BaseType"/>, while others may get more specific (e.g. a sensor of type "Ping").
        /// </summary>
        [XmlElement("type")]
        public string DisplayType
        {
            get { return Lazy(() => displayType); }
            set { displayType = value; }
        }

        private string internalType;

        [XmlElement("type_raw")]
        internal string type
        {
            get { return Lazy(() => internalType); }
            set { internalType = value; }
        }

        private StringEnum<ObjectType> enumType;

        /// <summary>
        /// The type of this object.
        /// </summary>
        [PropertyParameter(Property.Type)]
        public StringEnum<ObjectType> Type
        {
            get { return Lazy(() =>
            {
                if (enumType == null && !string.IsNullOrEmpty(type))
                {
                    if (baseType == BaseType.Sensor)
                        enumType = new StringEnum<ObjectType>(ObjectType.Sensor, type);
                    else
                        enumType = new StringEnum<ObjectType>(type);
                }

                return enumType;
            }); }
            set { enumType = value; }
        }

        internal SensorTypeInternal? typeRaw => (SensorTypeInternal?)EnumExtensions.XmlToEnum<XmlEnumAttribute>(Type?.StringValue, typeof(SensorTypeInternal), false);

        private bool active;

        /// <summary>
        /// Whether or not the object is currently active (in a monitoring state). If false, the object is paused.
        /// </summary>
        [XmlElement("active_raw")]
        [PropertyParameter(Property.Active)]
        public bool Active
        {
            get { return Lazy(() => active); }
            set { active = value; }
        }

        [XmlElement("basetype")]
        [PropertyParameter(Property.BaseType)]
        internal BaseType? baseType { get; set; }

        internal readonly string raw;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name ?? base.ToString();
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
        internal PrtgObject(string raw)
        {
            if (raw == null)
                throw new ArgumentNullException(nameof(raw));

            if (raw == "-1")
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

        internal static int GetId(string raw)
        {
            if (raw == null)
                throw new ArgumentNullException(nameof(raw));

            if (raw == "-1")
                return -1;

            var components = raw.Split('|');
            return Convert.ToInt32(components[0]);
        }

        [ExcludeFromCodeCoverage]
        string ISerializable.GetSerializedFormat()
        {
            return raw ?? ToString();
        }
        
        internal T Lazy<T>(Func<T> getValue) => this.Get(getValue);
    }
}
