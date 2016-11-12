using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Exceptions.Internal
{
    [Serializable]
    public class MissingAttributeException : Exception
    {
        public MissingAttributeException()
        {
        }

        public MissingAttributeException(Type type, string property) : base(GetMessage(type, property))
        {
        }

        public MissingAttributeException(Type type, string property, Exception inner) : base(GetMessage(type, property), inner)
        {
        }

        protected MissingAttributeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string GetMessage(Type type, string property)
        {
            return $"Property '{property}' of type '{type}' is missing a PSVisibleAttribute";
        }
    }
}
