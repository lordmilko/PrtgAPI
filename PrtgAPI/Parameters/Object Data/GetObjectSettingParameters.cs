namespace PrtgAPI.Parameters
{
    class GetObjectSettingParameters<T> : BaseObjectSettingParameters<T>
    {
        public GetObjectSettingParameters(int objectId, T name) : base(objectId, name)
        {
        }

        public string Show
        {
            get { return (string)this[Parameter.Show]; }
            set { this[Parameter.Show] = value; }
        }
    }
}
