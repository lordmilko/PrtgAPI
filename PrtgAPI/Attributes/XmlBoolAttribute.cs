using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal class XmlBoolAttribute : Attribute
    {
        public bool Positive { get; }

        internal XmlBoolAttribute(bool positive = true)
        {
            Positive = positive;
        }
    }
}
