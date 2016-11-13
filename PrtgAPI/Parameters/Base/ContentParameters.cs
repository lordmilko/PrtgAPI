using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    public class ContentParameters<T> : Parameters
    {
        protected static Property[] defaultProperties = GetDefaultProperties();

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
        /// Maximum number of records to return.
        /// </summary>
        public int Count
        {
            get { return (int)this[Parameter.Count]; }
            set { this[Parameter.Count] = value; }
        }

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
