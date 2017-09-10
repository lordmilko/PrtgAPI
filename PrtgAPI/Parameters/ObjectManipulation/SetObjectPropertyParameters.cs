using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Undocumented;

namespace PrtgAPI.Parameters
{
    sealed class SetObjectPropertyParameters : BaseSetObjectPropertyParameters<ObjectProperty>
    {
        //i think the properties in sensorsetting need an enum that links them to an object property
        //and then we use reflection to get the param with a given property and then confirm
        //that the value we were given is convertable to the given type

        //we should then implement this type safety for setchannelproperty as well

        //we need to add handling for inherit error interval

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
