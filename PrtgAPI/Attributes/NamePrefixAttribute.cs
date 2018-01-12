using System;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies that the Name of an object is just a prefix for a name that will be dynamically generated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    sealed class NamePrefixAttribute : Attribute
    {
        public NamePrefixAttribute()
        {
        }
    }
}
