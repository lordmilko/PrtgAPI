using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class NotificationTriggerParameters : BaseParameters, IXmlParameters
    {
        XmlFunction IXmlParameters.Function => XmlFunction.TableData;

        public NotificationTriggerParameters(Either<IPrtgObject, int> objectOrId)
        {
            ObjectId = objectOrId.GetId();
            this[Parameter.Content] = Content.Triggers;
            this[Parameter.Columns] = new[] { Property.Content, Property.Id };
        }

        public int ObjectId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public Content Content => (Content)this[Parameter.Content];

        public Property[] Properties => (Property[])this[Parameter.Columns];
    }
}
