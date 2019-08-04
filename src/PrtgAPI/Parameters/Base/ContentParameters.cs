using System.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Reflection;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving data with a known <see cref="Content"/> type.
    /// </summary>
    /// <typeparam name="T">The type of PRTG Object to retrieve.</typeparam>
    public abstract class ContentParameters<T> : PageableParameters, IXmlParameters
        where T : IObject
    {
        private static Property[] defaultProperties = GetDefaultProperties();

        XmlFunction IXmlParameters.Function => XmlFunction.TableData;

        /// <summary>
        /// Gets the type of content this request will retrieve.
        /// </summary>
        public Content Content => (Content)this[Parameter.Content];

        /// <summary>
        /// Gets the properties that will be retrieved for the objects returned from this request.
        /// </summary>
        public Property[] Properties
        {
            get { return (Property[])this[Parameter.Columns]; }
            internal set { this[Parameter.Columns] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentParameters{T}"/> class.
        /// </summary>
        protected ContentParameters(Content content)
        {
            this[Parameter.Content] = content;
            Properties = defaultProperties;
            Count = null;
        }

        internal static Property[] GetDefaultProperties()
        {
            var properties = typeof(T).GetTypeCache().Properties
                .Where(p => p.GetAttribute<UndocumentedAttribute>() == null)
                .Select(e => e.GetAttribute<PropertyParameterAttribute>())
                .Where(p => p != null)
                .Select(e => (Property)e.Property)
                .Distinct()
                .OrderBy(p => p != Property.Id && p != Property.Name) //Id and Name come first (if applicable)
                .ToArray();

            return properties;
        }
    }
}
