using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    sealed class SplittableStringAttribute : Attribute
    {
        public char Character { get; private set; }

        public SplittableStringAttribute(char character)
        {
            Character = character;
        }
    }
}
