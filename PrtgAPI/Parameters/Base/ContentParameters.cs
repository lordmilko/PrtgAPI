using System.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;
using PrtgAPI.Request;

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
        /// Maximum number of records to return. To retrieve all records in a single request, set this value to a very high number.<para/>
        /// If this value is less than the total number of records available, additional records can be obtained by requesting the next <see cref="Page"/>.
        /// </summary>
        public int? Count
        {
            get
            {
                var val = this[Parameter.Count];

                if (val == null)
                    return null;

                if (val.ToString() == "*")
                    return null;

                return (int) val;
            }
            set
            {
                if (value == null)
                    this[Parameter.Count] = "*";
                else
                    this[Parameter.Count] = value;
            }
        }

        /// <summary>
        /// Record number to start at.
        /// </summary>
        public int? Start
        {
            get { return (int?)this[Parameter.Start]; }
            set { this[Parameter.Start] = value; }
        }

        /// <summary>
        /// The page of the response to return. Requests can be split over multiple pages to increase the speed of each individual request.
        /// </summary>
        public int Page
        {
            get
            {
                if (Start == 0 || Start == null || Count == null)
                    return 1;
                return (Start.Value/Count.Value) + 1;
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
            Count = null;
        }

        static Property[] GetDefaultProperties()
        {
            return typeof(T).GetProperties()
                .Where(e => e.GetCustomAttributes(typeof(UndocumentedAttribute), false).Length == 0)
                .Select(e => e.GetCustomAttributes(typeof(PropertyParameterAttribute), false))
                .Where(el => el.Length > 0)
                .Select(elm => ((PropertyParameterAttribute)elm.First()).Name.ToEnum<Property>())
            .ToArray();
        }
    }
}
