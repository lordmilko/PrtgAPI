using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters.Helpers
{
    class TypeConversionStateMachine
    {
        private DynamicPropertyTypeParser parser;

        public Enum Property => parser.Property;

        public Type PropertyType => parser.PropertyType;

        public Type ValueType => parser.ValueType;

        public PropertyCache Cache => parser.Cache;

        public ConversionState State { get; set; }

        public object OriginalValue => parser.Value;

        public bool AllowNull => parser.AllowNull;

        public ConversionState ValueConversionWithMaybeNullCheck =>
            AllowNull ? ConversionState.ValueConversion : ConversionState.ValueConversionWithNullCheck;

        public object NewValue;

        public SerializationMode Mode { get; }

        public TypeCategory Type { get; set; }

        public TypeConversionStateMachine(DynamicPropertyTypeParser parser, SerializationMode mode)
        {
            this.parser = parser;
            this.Mode = mode;
        }

        public ConversionState MoveNext()
        {
            var newState = (ConversionState) (((int) State) + 1);

            Debug.Assert(newState != ConversionState.Completed);

            return newState;
        }

        public void Run()
        {
            while (State != ConversionState.Completed)
                State = RunMachine();
        }

        private ConversionState RunMachine()
        {
            switch (State)
            {
                case ConversionState.CorrectType:
                    return ProcessCorrectType();

                case ConversionState.Serializable:
                    return ProcessISerializable();

                case ConversionState.Enumerable:
                    return ProcessIEnumerable();

                case ConversionState.Nullable:
                    return ProcessNullable();

                case ConversionState.ImplicitConversion:
                    return ProcessImplicitConversion();

                case ConversionState.NonNullable:
                    return ProcessNonNullable();

                case ConversionState.ValueConversion:
                    return ProcessValueConverter(false);

                case ConversionState.ValueConversionWithNullCheck:
                    return ProcessValueConverter(true);

                default:
                    throw new NotImplementedException($"Don't know how to process state '{State}'.");
            }
        }

        #region CorrectType

        private ConversionState ProcessCorrectType()
        {
            if (Mode == SerializationMode.Deserialize)
            {
                //Shortcut out of deserialization mode
                if (PropertyType == ValueType)
                {
                    NewValue = OriginalValue;
                    return ConversionState.ValueConversion;
                }
            }

            if (Mode == SerializationMode.Serialize)
            {
                if (Property.Equals(ObjectProperty.PrimaryChannel))
                {
                    if (ValueType != typeof(Channel))
                        throw GetInvalidTypeException(typeof(Channel));

                    if (OriginalValue != null)
                    {
                        var channel = (Channel) OriginalValue;

                        NewValue = $"{channel.Id}|{channel.Name} ({channel.Unit})|";
                    }

                    return ConversionState.ValueConversionWithNullCheck;
                }
            }

            return MoveNext();
        }

        #endregion
        #region ISerializable

        private ConversionState ProcessISerializable()
        {
            if (Mode == SerializationMode.Serialize)
                return SerializeISerializable();
            else
                return DeserializeISerializable();
        }

        private ConversionState SerializeISerializable()
        {
            var expectedTypeAttribute = Property.GetEnumAttribute<TypeAttribute>();

            if (expectedTypeAttribute != null)
            {
                if (!typeof(ISerializable).IsAssignableFrom(expectedTypeAttribute.Class))
                    throw new InvalidOperationException($"Property '{Property}' has a {nameof(TypeAttribute)} of type '{expectedTypeAttribute.Class}' which does not implement '{typeof(ISerializable)}'. This represents an internal bug that must be corrected.");

                if (OriginalValue != null)
                {
                    if (expectedTypeAttribute.Class.IsAssignableFrom(ValueType))
                    {
                        Type = TypeCategory.Other;
                        NewValue = ((ISerializable) OriginalValue).GetSerializedFormat();
                        return ConversionState.ValueConversion;
                    }
                    else
                        throw GetInvalidTypeException(expectedTypeAttribute.Class);
                }
            }
            else
            {
                if (PropertyType == ValueType && typeof(ISerializable).IsAssignableFrom(PropertyType))
                {
                    Type = TypeCategory.Other;
                    NewValue = ((ISerializable)OriginalValue).GetSerializedFormat();
                    return ConversionState.ValueConversion;
                }
            }

            return MoveNext();
        }

        private ConversionState DeserializeISerializable()
        {
            var expectedTypeAttribute = Property.GetEnumAttribute<TypeAttribute>();

            if (expectedTypeAttribute != null)
            {
                if (!typeof(ISerializable).IsAssignableFrom(expectedTypeAttribute.Class))
                    throw new InvalidOperationException($"Property '{Property}' has a {nameof(TypeAttribute)} of type '{expectedTypeAttribute.Class}' which does not implement '{typeof(ISerializable)}'. This represents an internal bug that must be corrected.");

                if (OriginalValue != null)
                {
                    //If someone specifies the ACTUALLY serialized form of a value, we don't actually support deserializing that
                    //(e.g. "3 days" for a ScanningInterval, however as currently designed we don't actually use deserialization
                    //in DynamicPropertyTypeParser for anything but transforming "normal" user input
                    if (expectedTypeAttribute.Class.IsAssignableFrom(ValueType))
                    {
                        Type = TypeCategory.Other;
                        NewValue = OriginalValue;
                        return ConversionState.ValueConversion;
                    }
                    else
                        throw new InvalidTypeException($"Expected a value of type '{expectedTypeAttribute.Class}' while parsing property '{Property}' however received a value of type '{ValueType}'.");
                }
                else
                    return ConversionState.ValueConversionWithNullCheck;
                }

            return MoveNext();
        }

        #endregion
        #region IEnumerable

        private ConversionState ProcessIEnumerable()
        {
            if (Mode == SerializationMode.Serialize)
                return SerializeIEnumerable();
            else
                return DeserializeIEnumerable();
        }

        private ConversionState SerializeIEnumerable()
        {
            var supportedArrayTypes = new[]
            {
                typeof(object),
                typeof(string)
            };

            //We would like for our Value to be some type of IEnumerable so that we may serialize it
            //as a splittable string
            if (PropertyType.IsArray)
            {
                if (ValueType?.IsArray == true)
                {
                    var elementType = ValueType.GetElementType();

                    if (!supportedArrayTypes.Contains(elementType))
                        throw GetNotSupportedCollectionException(elementType);
                }

                //The serialized form of null is null. The serialized form of any list is a string
                if (OriginalValue == null || OriginalValue is string)
                {
                    NewValue = OriginalValue as string;
                    return ConversionState.ValueConversion;
                }

                if (OriginalValue.IsIEnumerable())
                {
                    foreach (var @interface in ValueType.GetInterfaces())
                    {
                        if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            var genericTypeArg = @interface.GetGenericArguments()[0];

                            if (!supportedArrayTypes.Contains(genericTypeArg))
                                throw GetNotSupportedCollectionException(genericTypeArg);

                            break;
                        }
                    }

                    var enumerable = OriginalValue.ToIEnumerable().ToArray();

                    if (enumerable.Length == 0)
                    {
                        NewValue = null;
                        return ConversionState.ValueConversion;
                    }

                    var illegalTypes = enumerable.Where(v => !(v is string)).ToList();

                    if (illegalTypes.Count > 0)
                    {
                        throw GetNotSupportedCollectionException(illegalTypes[0].GetType());
                    }

                    //The first character is considered canon. All subsequent characters are for alternate characters that
                    //could somehow get thrown into the mix
                    var splittableStringChar = Cache.GetAttribute<SplittableStringAttribute>(true)?.Characters[0].ToString();

                    if (splittableStringChar == null)
                        throw new NotSupportedException($"Cannot serialize value for array property {Property} as the property is missing a {nameof(SplittableStringAttribute)}.");

                    NewValue = string.Join(splittableStringChar, enumerable);
                    return ConversionState.ValueConversion;
                }

                throw GetInvalidTypeException();
            }

            return MoveNext();
        }

        private ConversionState DeserializeIEnumerable()
        {
            if (PropertyType.IsArray)
            {
                //If Value was string[] we would have shortcut out due to PropertyType == ValueType in ProcessCorrectType()

                if (OriginalValue == null)
                {
                    NewValue = null;
                    return ConversionState.ValueConversion;
                }

                if (OriginalValue.IsIEnumerable())
                {
                    NewValue = OriginalValue.ToIEnumerable().Where(v => v != null).Select(v => v.ToString()).ToArray();
                    return ConversionState.ValueConversion;
                }

                var characters = Cache.GetAttribute<SplittableStringAttribute>(true)?.Characters;

                if (characters != null)
                {
                    NewValue = OriginalValue.ToString().Split(characters);
                    return ConversionState.ValueConversion;
                }

                NewValue = new[]
                {
                    OriginalValue.ToString()
                };

                return ConversionState.ValueConversion;
            }

            return MoveNext();
        }

        #endregion
        #region Nullable

        private ConversionState ProcessNullable()
        {
            //String, Int and Double can be used as is
            if (PropertyType == typeof(string))
            {
                Type = TypeCategory.String;

                NewValue = OriginalValue?.ToString();

                return ValueConversionWithMaybeNullCheck;
            }
            else if (PropertyType == typeof(double) || PropertyType == typeof(int))
            {
                NewValue = ParseNumericValue();

                return ValueConversionWithMaybeNullCheck;
            }

            return MoveNext();
        }

        private object ParseNumericValue()
        {
            object val = null;

            //If the value is convertable to a double, it is either an int or a double
            if (!string.IsNullOrEmpty(OriginalValue?.ToString()))
            {
                double doubleResult;

                //If the value is a double. Implicitly not null due to !string.IsNullOrEmpty check above
                if (double.TryParse(OriginalValue.ToString(), out doubleResult))
                {
                    //If we're actually looking for an int, see if this double is actually an integer
                    if (PropertyType == typeof(int))
                    {
                        if (Convert.ToInt32(doubleResult) == doubleResult)
                        {
                            //If so, that's cool. When we ToString, we'll get an integer value anyway
                            Type = TypeCategory.Number;

                            if (Mode == SerializationMode.Deserialize)
                                val = (int)doubleResult;
                            else
                                val = doubleResult.ToString(CultureInfo.CurrentCulture);
                        }

                        //Else: someone tried to assign an actual double to our integer. An exception will be thrown below
                    }
                    else
                    {
                        Type = TypeCategory.Number;

                        if (Mode == SerializationMode.Deserialize)
                            val = doubleResult;
                        else
                        {
                            val = doubleResult.ToString(CultureInfo.CurrentCulture);

                            //Implication is the user assigned a value for the language used by the PRTG Server.
                            //Leave the value as is.
                            if (val != OriginalValue && OriginalValue is string)
                                return OriginalValue;
                        }
                    }
                }

                //If we still don't have a value, since we already verified our value is not null or empty we must have a value of an invalid type
                if (val == null)
                    throw GetInvalidTypeException();
            }

            return val;
        }

        #endregion
        #region NonNullable

        private ConversionState ProcessNonNullable()
        {
            if (OriginalValue == null)
                return ConversionState.ValueConversionWithNullCheck;

            if (PropertyType == typeof(bool))
                NewValue = ParseBoolValue();
            else if (PropertyType.IsEnum)
                NewValue = ProcessEnumValue();
            else
                throw GetInvalidValueException();

            if (NewValue == null)
                throw new ArgumentException($"Value '{OriginalValue}' could not be assigned to property '{Cache.Property.Name}'. Expected type: '{PropertyType}'. Actual type: '{ValueType}'.");

            return MoveNext();
        }

        private object ParseBoolValue()
        {
            object val = null;

            //If ValueType is bool, we must be serialize mode as in DeserializeMode we would have shortcut out of ParseValue
            //immediately due to PropertyType and ValueType both being bool
            if (ValueType == typeof(bool))
            {
                Type = TypeCategory.Boolean;

                val = Convert.ToInt32((bool)OriginalValue);
            }
            else
            {
                bool boolVal;

                //OriginalValue is implicitly not null because if it were ProcessNonNullable would have escaped
                //to ValueConversionWithNullCheck
                if (TryParseBool(OriginalValue.ToString(), out boolVal))
                {
                    Type = TypeCategory.Boolean;

                    if (Mode == SerializationMode.Deserialize)
                        val = boolVal;
                    else
                        val = Convert.ToInt32(boolVal);
                }
            }

            return val;
        }

        private bool TryParseBool(string value, out bool boolVal)
        {
            if (value == "0")
            {
                boolVal = false;
                return true;
            }

            if (value == "1")
            {
                boolVal = true;
                return true;
            }

            return bool.TryParse(value, out boolVal);
        }

        private object ProcessEnumValue()
        {
            object val = null;

            var useAlternateXml = Cache.GetAttribute<TypeLookupAttribute>()?.Class == typeof(XmlEnumAlternateName);

            if (useAlternateXml)
                val = ParseEnumValue<XmlEnumAlternateName>();
            else
                val = ParseEnumValue<XmlEnumAttribute>();

            if (val == null)
                throw GetEnumArgumentException();
            else
                Type = TypeCategory.Enum;

            return val;
        }

        private object ParseEnumValue<T>() where T : XmlEnumAttribute
        {
            object val = null;

            //If our value type was an enum, get its XmlEnumAttribute immediately
            if (PropertyType == ValueType)
            {
                //We must be serializing as if we were deserializing we would have fast-pathed out of ParseValue
                val = ((Enum)OriginalValue).GetEnumAttribute<T>(true).Name;
            }
            else
            {
                //Otherwise, our value may have been a string. See if any enum members are named after the specified value
                if (Enum.GetNames(PropertyType).Any(x => x.ToLower() == OriginalValue.ToString().ToLower()))
                {
                    var enumValue = ((Enum)Enum.Parse(PropertyType, OriginalValue.ToString(), true));

                    if (Mode == SerializationMode.Deserialize)
                        val = enumValue;
                    else
                        val = enumValue.GetEnumAttribute<T>(true).Name;
                }
                else
                {
                    //If the enum represents a set of numeric values and our value was an integer,
                    int result;

                    if (PropertyType.GetTypeCache().GetAttribute<NumericEnumAttribute>() != null && int.TryParse(OriginalValue.ToString(), out result))
                    {
                        var enumVal = Enum.Parse(PropertyType, OriginalValue.ToString());

                        if (Mode == SerializationMode.Deserialize)
                            val = enumVal;
                        else
                            val = ((Enum)enumVal).GetEnumAttribute<T>(true).Name;
                    }
                }
            }

            return val;
        }

        private Exception GetInvalidValueException()
        {
            if (PropertyType == ValueType)
            {
                //We must be serializing; if we were deserializing we would have taken the fast-path out of ParseValue.
                //And if the type did implement ISerializable we would have processed it in SerializeISerializable. Therefore,
                //this type is invalid.
                return new InvalidTypeException($"Cannot serialize value of type '{PropertyType}'; type does not implement '{typeof(ISerializable)}'.");
            }
            else
            {
                //Strictly speaking we could be deserializing a value that we'd like to pass to our Value's possible ValueConverter,
                //however we don't actually have any scenarios where we need to do that right now; deserialization is only used when adding
                //and modifying Notification Triggers in PowerShell, and in both cases we convert string values to NotificationActions before
                //they even get here. If you did Set-TriggerProperty NotificationAction $true, we don't want to let that reach our ValueConverter,
                //so we terminate things here

                return GetInvalidTypeException();
            }
        }

        #endregion
        #region ValueConverter

        private ConversionState ProcessValueConverter(bool checkNull)
        {
            var converter = Property.GetEnumAttribute<ValueConverterAttribute>();

            if (converter != null)
            {
                if (Mode == SerializationMode.Serialize)
                    NewValue = converter.Converter.Serialize(NewValue ?? OriginalValue);
                else
                    NewValue = converter.Converter.Deserialize(NewValue ?? OriginalValue);

                return ConversionState.Completed;
            }
            else
            {
                if (checkNull && OriginalValue == null)
                {
                    var type = Property.GetEnumAttribute<TypeAttribute>()?.Class;

                    throw GetInvalidNullException(type);
                }

                return ConversionState.Completed;
            }
        }

        #endregion
        #region ImplicitConversion

        private ConversionState ProcessImplicitConversion()
        {
            if (OriginalValue != null && ReflectionExtensions.IsPrtgAPIType(GetType(), PropertyType))
            {
                var implicitMethod = PropertyType.GetMethod("op_Implicit", new[] {ValueType});

                if (implicitMethod != null)
                {
                    NewValue = implicitMethod.Invoke(null, new object[] {OriginalValue});

                    return ConversionState.ValueConversion;
                }
            }

            return MoveNext();
        }

        #endregion
        #region Exception

        private Exception GetNotSupportedCollectionException(Type type)
        {
            return new NotSupportedException($"Properties containing collections of type '{type}' are not currently supported.");
        }

        private Exception GetInvalidTypeException(Type expectedType = null)
        {
            if (expectedType == null)
                expectedType = PropertyType;

            return new InvalidTypeException($"Value '{OriginalValue}' could not be assigned to property '{Cache.Property.Name}'. Expected type: '{expectedType}'. Actual type: '{ValueType}'.");
        }

        private Exception GetInvalidNullException(Type expectedType = null)
        {
            if (expectedType == null)
                expectedType = PropertyType;

            if(AllowNull)
                return new ArgumentNullException($"Value 'null' could not be assigned to property '{Cache.Property.Name}' of type '{expectedType}'. Null may only be assigned to properties of type '{typeof(string)}', '{typeof(int)}' and '{typeof(double)}'.", (Exception) null);
            else
                return new ArgumentNullException($"Value 'null' could not be assigned to property '{Cache.Property.Name}' of type '{expectedType}'. Value cannot be null.", (Exception) null);
        }

        private Exception GetEnumArgumentException()
        {
            var enumVals = Enum.GetValues(PropertyType).Cast<Enum>().ToList();

            var builder = new StringBuilder();

            for (int i = 0; i < enumVals.Count; i++)
            {
                builder.Append($"'{enumVals[i]}'");

                if (i < enumVals.Count - 2)
                    builder.Append(", ");
                else if (i == enumVals.Count - 2)
                    builder.Append(" or ");
            }

            return new ArgumentException($"'{OriginalValue}' is not a valid value for type '{PropertyType}'. Please specify one of {builder}.");
        }

        #endregion
    }
}
