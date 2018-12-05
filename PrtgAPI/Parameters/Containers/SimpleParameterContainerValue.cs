using System.Linq;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    class SimpleParameterContainerValue : IParameterContainerValue, IMultipleSerializable
    {
        public string Name { get; }

        public object Value { get; set; }

        public SimpleParameterContainerValue(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public void SetValue(object value, bool safe)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? "";
        }

        public string GetSerializedFormat()
        {
            return GetSerializedFormat(Value);
        }

        private string GetSerializedFormat(object value)
        {
            if (value is ISerializable)
                return ((ISerializable)value).GetSerializedFormat();

            return value?.ToString();
        }

        public string[] GetSerializedFormats()
        {
            if (Value.IsIEnumerable())
                return Value.ToIEnumerable().Select(GetSerializedFormat).ToArray();

            return new[] { GetSerializedFormat(Value) };
        }
    }
}
