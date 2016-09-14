namespace PrtgAPI.Parameters
{
    class PauseParametersBase : Parameters
    {
        protected PauseParametersBase(int objectId)
        {
            ObjectId = objectId;
        }

        public int ObjectId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public string PauseMessage
        {
            get { return (string) this[Parameter.PauseMessage]; }
            set { this[Parameter.PauseMessage] = value; }
        }
    }
}
