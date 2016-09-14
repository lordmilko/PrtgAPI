namespace PrtgAPI.Parameters
{
    class SetObjectSettingParameters<T> : BaseObjectSettingParameters<T>
    {
        public SetObjectSettingParameters(int objectId, T name, string value) : base(objectId, name)
        {
            Value = value;
        }

        public string Value
        {
            get { return (string)this[Parameter.Value]; }
            set { this[Parameter.Value] = value; }
        }
    }
}
