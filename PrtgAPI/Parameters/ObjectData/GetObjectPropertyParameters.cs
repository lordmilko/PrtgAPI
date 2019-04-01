using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class GetObjectPropertyParameters : BaseActionParameters, IHtmlParameters
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.ObjectData;

        public GetObjectPropertyParameters(Either<IPrtgObject, int> objectOrId, object objectType) : base(objectOrId)
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
