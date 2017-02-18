namespace PrtgAPI.Parameters
{
    class DeleteParameters : Parameters
    {
        public DeleteParameters(int objectId)
        {
            ObjectId = objectId;
            this[Parameter.Approve] = 1;
        }

        public int ObjectId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }
    }
}
