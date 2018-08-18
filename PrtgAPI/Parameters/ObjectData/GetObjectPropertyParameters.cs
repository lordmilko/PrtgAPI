using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class GetObjectPropertyParameters : BaseActionParameters, IHtmlParameters
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.ObjectData;

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
