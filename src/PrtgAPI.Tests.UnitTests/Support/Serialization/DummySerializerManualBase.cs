using System;
using System.Collections.Generic;
using System.Xml;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    abstract class DummySerializerManualBase<T> : XmlSerializerManual
    {
        object totalCount;
        object item;
        protected object property;

        public DummySerializerManualBase(XmlReader reader) : base(reader)
        {
        }

        protected override void Init()
        {
            totalCount = reader.NameTable.Add("totalcount");
            item = reader.NameTable.Add("item");
            property = reader.NameTable.Add("property");
        }

        public override object Deserialize(bool validateValueTypes)
        {
            return Deserialize<T>(2, ProcessHeaderAttributes, ProcessHeaderElements, ValidateHeader);
        }

        private bool ProcessHeaderAttributes(TableData<T> obj, bool[] flagArray)
        {
            //Flag 0 is TotalCount
            if (!flagArray[0] && AttributeName == totalCount)
            {
                obj.TotalCount = ToInt32(ReadAttributeString());
                flagArray[0] = true;
                return true;
            }

            return false;
        }

        private bool ProcessHeaderElements(TableData<T> obj, bool[] flagArray)
        {
            //Normally done within ReadElement
            if (obj.Items == null)
                obj.Items = new List<T>();

            //Flag 1 is Items, however because items is an array we allow it to be touched
            //multiple times
            if (ElementName == item)
            {
                obj.Items.Add(DeserializeItem());
            }

            return false;
        }

        private void ValidateHeader(bool[] flagArray)
        {
            if (!flagArray[0])
            {
                AttributeName = totalCount;
                throw Fail(null, null, typeof(int));
            }
        }

        protected abstract T DeserializeItem();

        protected abstract void ValidateDummy(bool[] flagArray);

        protected void ValidateSingleDummy<TValue>(bool[] flagArray)
        {
            if (!flagArray[0])
            {
                if (typeof(TValue).IsValueType && Nullable.GetUnderlyingType(typeof(TValue)) == null)
                {
                    ElementName = property;
                    throw Fail(null, null, typeof(TValue));
                }
            }
        }

        protected virtual TValue GetValue<TValue>(Func<string> getString)
        {
            if (typeof(TValue) == typeof(string))
                return (TValue)(object)ToNullableString(getString());
            if (typeof(TValue) == typeof(int))
                return (TValue)(object)ToInt32(getString());
            if (typeof(TValue) == typeof(int?))
                return (TValue)(object)ToNullableInt32(getString());
            if (typeof(TValue) == typeof(double))
                return (TValue)(object)ToDouble(getString());
            if (typeof(TValue) == typeof(double?))
                return (TValue)(object)ToNullableDouble(getString());
            if (typeof(TValue) == typeof(bool))
                return (TValue)(object)ToBool(getString());
            if (typeof(TValue) == typeof(bool?))
                return (TValue)(object)ToNullableBool(getString());
            if (typeof(TValue) == typeof(DateTime))
                return (TValue)(object)ToDateTime(getString());
            if (typeof(TValue) == typeof(DateTime?))
                return (TValue)(object)ToNullableDateTime(getString());
            if (typeof(TValue) == typeof(TimeSpan))
                return (TValue)(object)ToTimeSpan(getString());
            if (typeof(TValue) == typeof(TimeSpan?))
                return (TValue)(object)ToNullableTimeSpan(getString());
            if (typeof(TValue) == typeof(string[]))
                return (TValue)(object)ToSplittableStringArray(getString(), ' ');
            if (typeof(TValue) == typeof(Status))
                return (TValue)(object)ReadStatus(getString());
            if (typeof(TValue) == typeof(HttpMode))
                return (TValue)(object)ReadHttpMode(getString());
            if (typeof(TValue) == typeof(Status?))
                return (TValue)(object)ReadNullableStatus(getString());
            if (typeof(TValue) == typeof(AuthMode))
                return (TValue)(object)ReadAuthMode(getString());
            if (typeof(TValue) == typeof(PartialMissingXmlEnum))
                return (TValue)(object)ReadPartialMissingXmlEnum(getString());

            throw new NotImplementedException($"Don't know how to deserialize type '{typeof(TValue).Name}'");
        }

        private Status ReadStatus(string s)
        {
            switch (s)
            {
                case "0":
                    return Status.None;
                case "1":
                    return Status.Unknown;
                case "2":
                    return Status.Collecting;
                case "3":
                    return Status.Up;
                case "4":
                    return Status.Warning;
                case "5":
                    return Status.Down;
                case "6":
                    return Status.NoProbe;
                case "7":
                    return Status.PausedByUser;
                case "8":
                    return Status.PausedByDependency;
                case "9":
                    return Status.PausedBySchedule;
                case "10":
                    return Status.Unusual;
                case "11":
                    return Status.PausedByLicense;
                case "12":
                    return Status.PausedUntil;
                case "13":
                    return Status.DownAcknowledged;
                case "14":
                    return Status.DownPartial;
                default:
                    throw FailEnum(s, typeof(Status));
            }
        }

        private HttpMode ReadHttpMode(string s)
        {
            switch(s)
            {
                case "0":
                case "https":
                    return HttpMode.HTTPS;

                case "1":
                case "http":
                    return HttpMode.HTTP;
                default:
                    throw FailEnum(s, typeof(HttpMode));
            }
        }

        private AuthMode ReadAuthMode(string s)
        {
            switch(s)
            {
                default:
                    throw FailEnum(s, typeof(AuthMode));
            }
        }

        private PartialMissingXmlEnum ReadPartialMissingXmlEnum(string s)
        {
            switch (s)
            {
                case "WithAttribute":
                    return PartialMissingXmlEnum.WithAttribute;
                default:
                    throw FailEnum(s, typeof(PartialMissingXmlEnum));
            }
        }

        Status? ReadNullableStatus(string str)
        {
            return string.IsNullOrEmpty(str) ? null : (Status?)ReadStatus(str);
        }
    }
}
