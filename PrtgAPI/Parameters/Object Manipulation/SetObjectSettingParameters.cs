namespace PrtgAPI.Parameters
{
    class SetObjectSettingParameters<T> : BaseObjectSettingParameters<T>
    {
        public SetObjectSettingParameters(int objectId, T name, object value) : base(objectId, name)
        {
            Value = value;
        }

        public object Value
        {
            get { return this[Parameter.Value]; }
            set { this[Parameter.Value] = value; }
        }
    }
}
