using System.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving data with a known <see cref="Content"/> type.
    /// </summary>
    /// <typeparam name="T">The type of PRTG Object to retrieve.</typeparam>
    public class ContentParameters<T> : Parameters
    {
        private static Property[] defaultProperties = GetDefaultProperties();

        /// <summary>
        /// Maximum number of items that can be returned in a single request.
        /// </summary>
        private static readonly int MaxTableItems = 50000;

        /// <summary>
        /// The type of content this request will retrieve.
        /// </summary>
        public Content Content => (Content)this[Parameter.Content];

        /// <summary>
        /// Properties that will be retrieved for each PRTG Object.
        /// </summary>
        public Property[] Properties
        {
            get { return (Property[])this[Parameter.Columns]; }
            set { this[Parameter.Columns] = value; }
        }

        /// <summary>
        /// Maximum number of records to return.
        /// </summary>
        public int Count
        {
            get { return (int)this[Parameter.Count]; }
            set { this[Parameter.Count] = value; }
        }

        /// <summary>
        /// Record number to start at.
        /// </summary>
        public int? Start
        {
            get { return (int?)this[Parameter.Start]; }
            set { this[Parameter.Start] = value; }
        }

        public int Page
        {
            get
            {
                if (Start == 0 || Start == null)
                    return 1;
                return (Start.Value/Count) + 1;
            }
            set { Start = (value - 1)*Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentParameters{T}"/> class.
        /// </summary>
        protected ContentParameters(Content content)
        {
            this[Parameter.Content] = content;
            Properties = defaultProperties;
            Count = MaxTableItems;
        }

        static Property[] GetDefaultProperties()
        {
            return typeof(T).GetProperties()
                .Select(e => e.GetCustomAttributes(typeof(PropertyParameterAttribute), false))
                .Where(el => el.Length > 0)
                .Select(elm => ((PropertyParameterAttribute)elm.First()).Name.ToEnum<Property>())
            .ToArray();
        }
    }
}
