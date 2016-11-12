using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class GetObjectNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public GetObjectNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
