using System;
using System.Reflection;

namespace PrtgAPI.Parameters
{
    sealed class SetObjectPropertyParameters : BaseSetObjectPropertyParameters<ObjectProperty>
    {
        public int ObjectId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public SetObjectPropertyParameters(int objectId, ObjectProperty property, object value)
        {
            ObjectId = objectId;
            AddTypeSafeValue(property, value, false);
        }

        public SetObjectPropertyParameters(int objectId, string property, string value)
        {
            ObjectId = objectId;
            this[Parameter.Custom] = new CustomParameter(property, value);
        }

        protected override PropertyInfo GetPropertyInfo(Enum property)
        {
            return GetPropertyInfoViaTypeLookup(property);
        }
    }
}
