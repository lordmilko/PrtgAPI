using System;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    sealed class PSVisibleAttribute : Attribute
    {
        public bool Visible { get; private set; }

        public PSVisibleAttribute(bool visible)
        {
            Visible = visible;
        }
    }
}