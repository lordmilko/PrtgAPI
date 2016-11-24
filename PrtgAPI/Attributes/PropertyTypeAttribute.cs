using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class PropertyTypeAttribute : Attribute
    {
        public Type Type { get; private set; }

        public PropertyTypeAttribute(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException($"Type {enumType} must be an enum");

            this.Type = enumType;
        }
    }
}
