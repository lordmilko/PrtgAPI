using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class GetObjectPropertyRawParameters : BaseActionParameters, IXmlParameters
    {
        private static readonly string[] alternate = {"comments"};

        XmlFunction IXmlParameters.Function
        {
            get
            {
                if (IsAlternate(Name))
                    return XmlFunction.GetObjectStatus;

                return XmlFunction.GetObjectProperty;
            }
        }

        public GetObjectPropertyRawParameters(Either<IPrtgObject, int> objectOrId, string property, bool text) : base(objectOrId)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property), "Property cannot be null.");

            if (string.IsNullOrWhiteSpace(property))
                throw new ArgumentException("Property cannot be empty or whitespace.", nameof(property));

            if (property.EndsWith("_"))
                property = property.Substring(0, property.Length - 1);

            Name = property;

            if (text)
                this[Parameter.Show] = CustomValueFormat.Text;
        }

        public GetObjectPropertyRawParameters(Either<IPrtgObject, int> objectOrId, int subId, string subType, string property, bool text) : this(objectOrId, property, false)
        {
            if (subType == null)
                throw new ArgumentNullException(nameof(subType), "SubType cannot be null.");

            if (string.IsNullOrWhiteSpace(subType))
                throw new ArgumentException("SubType cannot be empty or whitespace.", nameof(subType));

            this[Parameter.SubId] = subId;
            this[Parameter.SubType] = subType;

            if (!text)
                this[Parameter.Show] = CustomValueFormat.NoHtmlEncode;
        }

        public string Name
        {
            get { return (string)this[Parameter.Name]; }
            set { this[Parameter.Name] = value; }
        }

        internal static bool IsAlternate(string property)
        {
            //Some properties do not display their values properly when retrieved by the default
            //API endpoint. For these, the alternate endpoint can be used instead.
            return alternate.Contains(property.TrimEnd('_').ToLower());
        }
    }
}
