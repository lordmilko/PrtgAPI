namespace PrtgAPI.Parameters
{
    class NotificationTriggerParameters : Parameters
    {
        public NotificationTriggerParameters(int objectId)
        {
            ObjectId = objectId;
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
