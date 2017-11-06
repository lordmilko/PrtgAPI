namespace PrtgAPI.Parameters
{
    class TotalObjectsParameters : Parameters
    {
        public TotalObjectsParameters(Content content)
        {
            Content = content;

            //Logs take longer if you ask for 0
            if (content == Content.Messages)
            {
                this[Parameter.Count] = 1;

                this[Parameter.Columns] = new[]
                {
                    Property.Id, Property.Name
                };
            }
            else
                this[Parameter.Count] = 0;

            if(content == Content.ProbeNode)
                this[Parameter.FilterXyz] = new SearchFilter(Property.ParentId, "0");
        }

        public Content Content
        {
            get { return (Content)this[Parameter.Content]; }
            set { this[Parameter.Content] = value; }
        }
    }
}
