using System;
using PrtgAPI.Request.Serialization.FilterHandlers;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies a custom <see cref="FilterHandler"/> used to validate the components specified in a <see cref="SearchFilter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    class FilterHandlerAttribute : Attribute
    {
        private Type type;

        private FilterHandler handler;

        public FilterHandler Handler => handler ?? (handler = (FilterHandler)Activator.CreateInstance(type));

        public FilterHandlerAttribute(Type type)
        {
            this.type = type;
        }
    }
}
