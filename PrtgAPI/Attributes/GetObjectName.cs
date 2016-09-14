using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    sealed class GetObjectName : Attribute
    {
        public GetObjectName(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
