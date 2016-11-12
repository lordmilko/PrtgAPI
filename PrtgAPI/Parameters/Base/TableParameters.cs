using System.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="T:PrtgAPI.PrtgUrl"/> for retrieving data stored in tables.
    /// </summary>
    /// <typeparam name="T">The type of PRTG Object to retrieve.</typeparam>
    public class TableParameters<T> : Parameters where T : SensorOrDeviceOrGroupOrProbe
    {
        private static Property[] defaultProperties = GetDefaultProperties();

        /// <summary>
        /// Maximum number of items that can be returned in a single request.
        /// </summary>
        public static readonly int MaxTableItems = 50000;

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
        /// Filter objects to those with a <see cref="T:PrtgAPI.Property"/> of a certain value. Specify multiple filters to limit results further.
        /// </summary>
        public virtual SearchFilter[] SearchFilter
        {
            get { return (SearchFilter[])this[Parameter.FilterXyz]; }
            set { this[Parameter.FilterXyz] = value; }
        }

        /// <summary>
        /// <see cref="T:PrtgAPI.Property"/> to sort response by.
        /// </summary>
        public Property SortBy
        {
            get { return (Property)this[Parameter.SortBy]; }
            set { this[Parameter.SortBy] = value; }
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
        /// Initializes a new instance of the <see cref="T:PrtgAPI.Parameters.TableParameters`1"/> class.
        /// </summary>
        protected TableParameters(Content content)
        {
            this[Parameter.Content] = content;
            Properties = defaultProperties;
            Count = MaxTableItems;
        }

        static Property[] GetDefaultProperties()
        {
            return typeof (T).GetProperties()
                .Select(e => e.GetCustomAttributes(typeof (PropertyParameterAttribute), false))
                .Where(el => el.Length > 0)
                .Select(elm => ((PropertyParameterAttribute)elm.First()).Name.ToEnum<Property>())
            .ToArray();            
        }
    }
}
