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

        public GetObjectPropertyRawParameters(int objectId, string name, bool text) : base(objectId)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty", nameof(name));

            if (name.EndsWith("_"))
                name = name.Substring(0, name.Length - 1);

            Name = name;

            if (text)
                this[Parameter.Show] = CustomValueFormat.Text;
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
