using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class GetObjectPropertyParameters : BaseActionParameters
    {
        public GetObjectPropertyParameters(int objectId, ObjectType objectType) : base(objectId)
        {
            ObjectType = objectType;
        }

        public ObjectType ObjectType
        {
            get { return this[Parameter.ObjectType].ToString().DescriptionToEnum<ObjectType>(); }
            set { this[Parameter.ObjectType] = value.GetDescription(); }
        }
    }
}
