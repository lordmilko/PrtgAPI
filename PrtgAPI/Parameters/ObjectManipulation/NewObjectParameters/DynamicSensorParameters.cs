using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Dynamic;
using PrtgAPI.Reflection;
using PrtgAPI.Request;
using PrtgAPI.Targets;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents a dynamic set of raw parameters used to construct a <see cref="PrtgRequestMessage"/> for creating a new sensor.
    /// </summary>
    public class DynamicSensorParameters : ContainerSensorParameters, IDynamicMetaObjectProvider, ISourceParameters<Device>
    {
        private const string TempName = "TEMP_NAME";

        /// <summary>
        /// Provides access to all sensor targets identified for this object.
        /// </summary>
        public Dictionary<string, GenericSensorTarget[]> Targets { get; private set; }

        private bool locked = true;

        /// <summary>
        /// Gets or sets the source device these parameters were derived from. If these parameters were not derived from a specific device this value is null.
        /// </summary>
        public Device Source { get; internal set; }

        /// <summary>
        /// Locks this object's parameters, preventing new parameters from being added via its indexer. If an attempt is made to set a nonexistant parameter while this object is locked, an <see cref="InvalidOperationException"/> will be thrown.
        /// </summary>
        public void Lock() { locked = true; }

        /// <summary>
        /// Unlocks this object's parameters, allowing new parameters to be added via its indexer. Attempting to retrieve the value of a nonexistant parameter will generate an <see cref="InvalidOperationException"/>.
        /// </summary>
        public void Unlock() { locked = false; }

        /// <summary>
        /// Indicates whether this object allows new properties to be added via its indexer.
        /// </summary>
        /// <returns>Whether new properties can be added</returns>
        public bool IsLocked() => locked;

        /// <summary>
        /// Determines whether a parameter is in the underlying parameter set.
        /// </summary>
        /// <param name="name">The name of the parameter to locate.</param>
        /// <param name="ignoreUnderscore">Specifies whether to ignore trailing underscores of both names in the comparison.</param>
        /// <returns>True if any parameters exist with the specified name; otherwise false.</returns>
        public bool Contains(string name, bool ignoreUnderscore = true) => ContainsInternal(name, ignoreUnderscore);

        /// <summary>
        /// Removes all occurrences of a specified parameter from the underlying parameter set.
        /// Applies regardless of whether this object <see cref="IsLocked"/>.
        /// </summary>
        /// <param name="name">The name of the parameter to remove.</param>
        /// <param name="ignoreUnderscore">Specifies whether to ignore trailing underscores of both names in the comparison.</param>
        /// <returns>True if one or more items were successfully removed. If no items exist with the specified name, this method returns false.</returns>
        public bool Remove(string name, bool ignoreUnderscore = true) => RemoveInternal(name, ignoreUnderscore);

        internal DynamicSensorParameters(string response, string sensorType) : base(TempName, sensorType, true)
        {
            using (ConstructorScope)
            {
                ParseResponse(response);

                Debug.Assert(Name != TempName, "Response did not contain the object's name. Parameters are in an invalid state.");
            } 
        }

        private void ParseResponse(string response)
        {
            var nameRegex = "(.+?name=\")(.+?_*)(\".+)";
            var inputs = HtmlParser.Default.GetFilteredInputs(response, nameRegex).Where(n => n.Name != "tmpid" && n.Name != "id" && n.Name != "sensortype" && n.Name != "parenttags_").ToList();
            var lists = HtmlParser.Default.GetDropDownList(response, nameRegex);
            var text = HtmlParser.Default.GetTextAreaFields(response, nameRegex);

            Targets = GenericSensorTarget.GetAllTargets(response).ToDictionary(t => t.Key, t => t.Value.ToArray());

            foreach (var input in inputs)
                AddCustom(input.Name, CleanValue(input.Value));

            foreach (var list in lists)
                AddCustom(list.Name, CleanValue(list.Options.FirstOrDefault(o => o.Selected)?.Value)); //todo: does this crash if its null?

            foreach (var t in text)
                Add(t.Key, CleanValue(t.Value));
        }

        private string CleanValue(string value)
        {
            if (value == string.Empty)
                return null;

            return value;
        }

        private void AddCustom(string name, string defaultValue)
        {
            var propKey = GetKey(TypedProperties, name);

            if (propKey != null) //Deserialize a value
            {
                var prop = TypedProperties[propKey];

                var deserialized = ILazyExtensions.Serializer.DeserializeObjectProperty(prop.Item1, defaultValue);

                prop.Item2.SetValue(this, deserialized);

                if (propKey.EndsWith("_") && !name.EndsWith("_"))
                    AddNameOverride(prop.Item1, name);

                return;
            }

            var targetKey = GetKey(Targets, name);

            if (targetKey != null) //Add a target
            {
                var targets = Targets[targetKey];

                Add(name, targets.FirstOrDefault());

                return;
            }

            Add(name, defaultValue);
        }

        internal override void SetIndex(string name, object value)
        {
            if (TrySet(TypedParameters, name, value, (v, t) => this[t.Item1] = value))
                return;

            var prop = InternalParameters.Where(p => HasName(p, name)).ToList();

            if (prop.Count == 0 && IsLocked())
                throw new InvalidOperationException($"Parameter with name '{name}' does not exist. To add new parameters object must first be unlocked.");

            if (IsSensorTarget(value))
            {
                foreach (var obj in prop)
                    InternalParameters.Remove(obj);

                Add(name, value);
            }
            else
            {
                SetOrAddCustom(prop, name, value);
            }
        }

        private bool IsSensorTarget(object val)
        {
            if (val == null)
                return true;

            if (val.GetType().IsSubclassOfRawGeneric(typeof(SensorTarget<>)))
                return true;

            if (val.IsIEnumerable() && val.ToIEnumerable().All(o => o.GetType().IsSubclassOfRawGeneric(typeof(SensorTarget<>))))
                return true;

            return false;
        }

        private string GetKey<TValue>(Dictionary<string, TValue> dictionary, string name)
        {
            return dictionary.Keys.FirstOrDefault(k => k.TrimEnd('_').ToUpperInvariant() == name.TrimEnd('_').ToUpperInvariant());
        }

        /// <summary>
        /// Provides access to the <see cref="DynamicMetaObject"/> that dispatches to this object's dynamic members.
        /// </summary>
        /// <param name="parameter">The expression that represents the <see cref="DynamicMetaObject"/> to dispatch to the dynamic members.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> that dispatches to this object's dynamic members.</returns>
        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicMetaObject<IDynamicParameters>(parameter, this, new ParameterProxy());
        }
    }
}
