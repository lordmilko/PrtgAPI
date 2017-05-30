namespace PrtgAPI.Parameters
{
    class BaseActionParameters : Parameters
    {
        public BaseActionParameters(int objectId)
        {
            ObjectId = objectId;
        }

        public int ObjectId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }
    }
}
