using System;
using System.Diagnostics.CodeAnalysis;

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
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty", nameof(name));

            if (name.EndsWith("_"))
                name = name.Substring(0, name.Length - 1);

            Name = name;
        }

        public string Name
        {
            get { return (string)this[Parameter.Name]; }
            set { this[Parameter.Name] = value; }
        }
    }
}
