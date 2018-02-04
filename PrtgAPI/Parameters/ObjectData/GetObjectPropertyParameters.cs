using System.Diagnostics.CodeAnalysis;

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
            get { return (ObjectType) this[Parameter.ObjectType]; }
            set { this[Parameter.ObjectType] = value; }
        }
    }
}
