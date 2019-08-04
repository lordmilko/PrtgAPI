using System;

namespace PrtgAPI.Request.Serialization.ValueConverters
{
    abstract class ValueConverter<T> : IValueConverter<T>
    {
        protected ValueConverter()
        {
        }

        public virtual object Serialize(object value)
        {
            return Serialize(value, Serialize);
        }

        private object Serialize(object value, Func<T, object> action)
        {
            T outVal;

            if (value != null && Convert(value, out outVal))
                return action(outVal);

            return value;
        }

        public string SerializeWithPadding(object value, bool pad)
        {
            return Serialize(value, v => ((IZeroPaddingConverter)this).Pad(v, pad))?.ToString();
        }

        public virtual object Deserialize(object value) => value != null ? (object) Deserialize((T) value) : null;

        public abstract string Serialize(T value);

        protected abstract T SerializeWithinType(T value);

        public abstract T Deserialize(T value);

        protected abstract bool Convert(object value, out T outVal);
    }
}
