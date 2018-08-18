using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class GetObjectPropertyParameters : BaseActionParameters, IHtmlParameters
    {
        public GetObjectPropertyParameters(int objectId, ObjectType objectType) : base(objectId)
        public GetObjectPropertyParameters(int objectId, object objectType) : base(objectId)
        {
            if (objectType != null)
                ObjectType = objectType;
        }

        public object ObjectType
        {
            get { return this[Parameter.ObjectType]; }
            set { this[Parameter.ObjectType] = value; }
        }
    }
}
