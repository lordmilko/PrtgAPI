using System;
using System.Xml.Serialization;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies the type of object that should be used in conjunction with a corresponding field.<para/>
    /// When used in conjunction with a <see cref="TypeLookupAttribute"/>, this attribute overrides the type
    /// indicated by the field found via the <see cref="TypeLookupAttribute"/>, indicating the <see cref="TypeLookupAttribute"/>
    /// is only being used for identifying other pieces of information, such as the value of a <see cref="XmlElementAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    sealed class TypeAttribute : Attribute
    {
        public TypeAttribute(Type @class)
        {
            Class = @class;
        }

        public Type Class { get; private set; }
    }
}
