using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    sealed class MutuallyExclusiveAttribute : Attribute
    {
        public string Name { get; private set; }

        public MutuallyExclusiveAttribute(string name)
        {
            Name = name;
        }
    }
}
