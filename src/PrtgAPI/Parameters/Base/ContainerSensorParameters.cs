using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using PrtgAPI.Attributes;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Request;
using PrtgAPI.Targets;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// <para type="description">Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for adding new <see cref="Sensor"/> objects,
    /// capable of functioning as both a <see cref="Dictionary{TKey, TValue}"/> and a secondary container type.</para>
    /// </summary>
    public abstract class ContainerSensorParameters : NewSensorParameters, IDynamicParameters
    {
        IParameterContainer container;

        private bool trimName;

        internal Dictionary<string, Tuple<ObjectProperty, PropertyInfo>> TypedProperties { get; } = GetTypedProperties();
        internal Dictionary<string, Tuple<Parameter, PropertyInfo>> TypedParameters { get; } = GetTypedParameters();

        internal bool ContainsInternal(string name, bool ignoreUnderscore) => InternalParameters.Any(p => HasName(p, name, ignoreUnderscore));

        internal bool RemoveInternal(string name, bool ignoreUnderscore)
        {
            var toRemove = InternalParameters.Where(p => HasName(p, name, ignoreUnderscore)).ToList();

            if (toRemove.Count == 0)
                return false;

            foreach (var obj in toRemove)
                InternalParameters.Remove(obj);

            return true;
        }

        private static Dictionary<string, Tuple<ObjectProperty, PropertyInfo>> GetTypedProperties()
        {
            return GetPropertyDictionary<ObjectProperty>(ObjectPropertyParser.GetObjectPropertyName);
        }

        private static Dictionary<string, Tuple<Parameter, PropertyInfo>> GetTypedParameters()
        {
            var bad = GetTypedProperties();
            var good = GetPropertyDictionary<Parameter>(e => e.GetDescription()).Where(
                c => bad.All(b => b.Value.Item1.ToString() != c.Value.Item1.ToString())
            ).ToDictionary(b => b.Key, b => b.Value);

            return good;
        }

        private static Dictionary<string, Tuple<TEnum, PropertyInfo>> GetPropertyDictionary<TEnum>(Func<TEnum, string> getName) where TEnum : struct
        {
            var ps = typeof(DynamicSensorParameters).GetTypeCache().Properties;

            var dict = ps.Select(GetTuple<TEnum>).Where(t => t != null).ToDictionary(
                t => getName(t.Item2),
                t => Tuple.Create(t.Item2, t.Item1)
            );

            return dict;
        }

        private static Tuple<PropertyInfo, TEnum, PropertyParameterAttribute> GetTuple<TEnum>(PropertyCache cache) where TEnum : struct
        {
            var attr = cache.GetAttribute<PropertyParameterAttribute>();

            if (attr == null)
                return null;

            if (attr.Property.GetType() == typeof(TEnum))
                return Tuple.Create(cache.Property, (TEnum)(object)attr.Property, attr);

            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerSensorParameters"/> class.
        /// </summary>
        /// <param name="sensorName">The name to use for this sensor.</param>
        /// <param name="sensorType">The type of sensor these parameters will create.</param>
        /// <param name="trimName">Whether to trim trailing underscores when doing parameter name lookups.</param>
        protected ContainerSensorParameters(string sensorName, string sensorType, bool trimName) : base(sensorName, sensorType)
        {
            container = PSObjectUtilities.Instance.GetContainer();
            container.Initialize(this);
            this.trimName = trimName;
        }

        /// <summary>
        /// Gets or sets the type of sensor these parameters will create.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(Parameter.SensorType)]
        public string SensorType
        {
            get { return this[Parameter.SensorType]?.ToString(); }
            set { this[Parameter.SensorType] = value; }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified parameter.
        /// </summary>
        /// <param name="name">The name of the parameter to get or set.</param>
        /// <returns>When getting, if the specified parameter exiasts, the value of that parameter.
        /// If the parameter does not exist, an <see cref="InvalidOperationException"/> will be thrown.</returns>
        public object this[string name]
        {
            get { return GetIndex(name); }
            set
            {
                value = PSObjectUtilities.CleanPSObject(value);

                SetIndex(name, value);
            }
        }

        private object GetIndex(string name)
        {
            Tuple<Parameter, PropertyInfo> param;
            Tuple<ObjectProperty, PropertyInfo> prop;

            if (TypedParameters.TryGetValue(name, out param, true))
                return this[param.Item1];
            if (TypedProperties.TryGetValue(name, out prop, true, trimName))
                return prop.Item2.GetValue(this);            

            var matches = InternalParameters
                .Where(p => HasName(p, name, trimName))
                .Select(p =>
                {
                    if (p.Value is IParameterContainerValue)
                        return ((IParameterContainerValue)p.Value).Value;

                    return p.Value;
                })
                .ToList();

            if (matches.Count == 1)
                return matches.First();

            if (matches.Count > 0)
            {
                if (matches.All(o => o is GenericSensorTarget))
                    return matches.Cast<GenericSensorTarget>().ToArray();

                throw new NotSupportedException($"Property '{name}' contains an invalid collection of elements.");
            }

            throw new InvalidOperationException($"Parameter with name '{name}' does not exist.");
        }

        internal virtual void SetIndex(string name, object value)
        {
            if (TrySet(TypedParameters, name, value, (v, t) => this[t.Item1] = value))
                return;
            if (TrySet(TypedProperties, name, value, (v, t) => t.Item2.SetValue(this, v)))
                return;

            var props = InternalParameters.Where(p => HasName(p, name, trimName)).ToList();

            SetOrAddCustom(props, name, value);
        }

        internal bool TrySet<T>(Dictionary<string, Tuple<T, PropertyInfo>> dictionary, string name, object value, Action<object, Tuple<T, PropertyInfo>> setValue)
        {
            Tuple<T, PropertyInfo> tuple;

            if (dictionary.TryGetValue(name, out tuple, true, trimName))
            {
                if (typeof(T) == typeof(ObjectProperty))
                {
                    var val = ILazyExtensions.Serializer.DeserializeObjectProperty((ObjectProperty) (object) tuple.Item1, value?.ToString());

                    setValue(val, tuple);
                }
                else
                    setValue(value, tuple);
                return true;
            }

            return false;
        }

        internal void SetOrAddCustom(List<CustomParameter> prop, string name, object value)
        {
            if (prop.Count > 0)
            {
                var singleProp = prop.First();

                var cv = singleProp.Value as IParameterContainerValue;

                if (cv != null) //We're a custom parameter, like exeparams
                    cv.SetValue(value, true);
                else
                {
                    if (TrySet(TypedProperties, name, value, (v, t) => t.Item2.SetValue(this, v)))
                        return;

                    Debug.Assert(false, "We should never get here");
                }
            }
            else //We must be unlocked adding a new property
                Add(name, value);
        }

        internal void Add(string name, object value)
        {
            var param = new CustomParameter(name, null, ParameterType.MultiParameter);

            var parameterValue = container.CreateValue(name, value, trimName);

            param.Value = parameterValue;

            InternalParameters.Add(param);
            container.Add(parameterValue);
        }

        internal bool HasName(CustomParameter param, string name, bool ignoreUnderscore = true)
        {
            if (ignoreUnderscore)
                return param.Name.TrimEnd('_').ToUpperInvariant() == name.TrimEnd('_').ToUpperInvariant();

            return param.Name.ToUpperInvariant() == name.ToUpperInvariant();
        }
    }
}
