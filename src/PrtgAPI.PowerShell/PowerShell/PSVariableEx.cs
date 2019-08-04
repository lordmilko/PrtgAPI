using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell
{
    [ExcludeFromCodeCoverage]
    class PSVariableEx : PSVariable, IMultipleSerializable, IParameterContainerValue
    {
        internal string RawName { get; set; }

        private Action<object> setValue;

        public PSVariableEx(string name, object initial, Action<object> setValue, bool trimName = true) : base(GetName(name, trimName), initial)
        {
            RawName = name;

            this.setValue = setValue;
        }

        private static string GetName(string name, bool trimName)
        {
            if (trimName)
                return name.TrimEnd('_');

            return name;
        }

        public override object Value
        {
            get { return base.Value; }
            set { ((IParameterContainerValue)this).SetValue(value, false); }
        }

        void IParameterContainerValue.SetValue(object value, bool safe)
        {
            if (safe)
                base.Value = value;
            else
                setValue(value);
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
