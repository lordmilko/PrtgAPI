using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Base class for all parameters that create new table objects.
    /// </summary>
    public abstract class NewObjectParameters : BaseParameters, ICommandParameters, ICustomParameterContainer, IPropertyCacheResolver
    {
        internal abstract CommandFunction Function { get; }

        internal Dictionary<ObjectProperty, string> nameOverrideMap = new Dictionary<ObjectProperty, string>();

        CommandFunction ICommandParameters.Function => Function;

        private ConstructorScope constructorScope;

        /// <summary>
        /// Protects the constructor from setting dependent properties when initializing the object.
        /// </summary>
        protected IDisposable ConstructorScope
        {
            get
            {
                constructorScope = new ConstructorScope();

                return constructorScope;
            }
        }

        ObjectPropertyParser parser;

        /// <summary>
        /// Gets or sets the name to use for this object.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(ObjectProperty.Name)]
        public string Name
        {
            get { return GetCustomParameter(ObjectProperty.Name)?.ToString(); }
            set { SetCustomParameter(ObjectProperty.Name, value); }
        }

        /// <summary>
        /// Gets or sets the tags that should be applied to this object. Certain object types and subtypes (such as sensors) may have default tag values.<para/>
        /// If this value is null, the default tags based on sensor type will be used.
        /// </summary>
        [PropertyParameter(ObjectProperty.Tags)]
        public string[] Tags
        {
            get { return GetCustomParameterArray(ObjectProperty.Tags, ' '); }
            set { SetCustomParameterArray(ObjectProperty.Tags, value, ' '); }
        }

        internal NewObjectParameters(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name), "An object name cannot be null.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));

            parser = new ObjectPropertyParser(this, this, ObjectPropertyParser.GetObjectPropertyNameViaCache);

            Name = name;
        }

        #region GetCustomParameter

        /// <summary>
        /// Retrieves the value of a property from the underlying parameter set.
        /// </summary>
        /// <param name="property">The property whose value should be retrieved</param>
        /// <returns>The original unserialized value.</returns>
        protected object GetCustomParameter(ObjectProperty property) =>
            GetCustomParameterInternal(GetObjectPropertyName(property));

        [ExcludeFromCodeCoverage]
        internal object GetCustomParameter(ObjectPropertyInternal property) =>
            GetCustomParameterInternal(GetObjectPropertyName(property));

        #endregion
        #region GetCustomParameterEnumXml

        /// <summary>
        /// Retrieves the original value of a serialized enum property from the underlying parameter set.
        /// </summary>
        /// <typeparam name="T">The type of enum to retrieve.</typeparam>
        /// <param name="property">The property whose value should be retrieved.</param>
        /// <returns>The original unserialized value.</returns>
        protected object GetCustomParameterEnumXml<T>(ObjectProperty property) =>
            GetCustomParameterEnumXml<T>(GetObjectPropertyName(property));

        [ExcludeFromCodeCoverage]
        internal object GetCustomParameterEnumXml<T>(ObjectPropertyInternal property) =>
            GetCustomParameterEnumXml<T>(GetObjectPropertyName(property));

        /// <summary>
        /// Retrieves the original value of a serialized enum property from the underlying parameter set, using its raw serialized name.
        /// </summary>
        /// <typeparam name="T">The type of enum to retrieve.</typeparam>
        /// <param name="name">The raw name of the parameter</param>
        /// <returns>The original unserialized value.</returns>
        protected object GetCustomParameterEnumXml<T>(string name)
        {
            var value = GetCustomParameterInternal(name);

            if (value == null)
                return null;

            return value.ToString().XmlToEnum<T>();
        }

        #endregion
        #region GetCustomParameterBool

        /// <summary>
        /// Retrieves the original value of a serialized boolean property from the underlying parameter set.
        /// </summary>
        /// <param name="property">The property whose value should be retrieved.</param>
        /// <returns>The original unserialized value.</returns>
        protected bool? GetCustomParameterBool(ObjectProperty property) =>
            GetCustomParameterBool(GetObjectPropertyName(property));

        [ExcludeFromCodeCoverage]
        internal bool? GetCustomParameterBool(ObjectPropertyInternal property) =>
            GetCustomParameterBool(GetObjectPropertyName(property));

        /// <summary>
        /// Retrieves the original value of a serialized boolean property from the underlying parameter set, using its raw serialized name.
        /// </summary>
        /// <param name="name">The raw name of the parameter</param>
        /// <returns>The original unserialized value.</returns>
        protected bool? GetCustomParameterBool(string name)
        {
            var value = GetCustomParameterInternal(name);

            if (value == null)
                return null;

            return Convert.ToBoolean(value);
        }

        #endregion
        #region GetCustomParameterArray

        /// <summary>
        /// Retrieves the original value of a serialized array property from the underlying parameter set.
        /// </summary>
        /// <param name="property">The parameter to retrieve</param>
        /// <param name="delim">The value that was used to combine the array when originally serialized via <see cref="SetCustomParameterArray(ObjectProperty, string[], char[])"/> </param>
        /// <returns>The original unserialized value.</returns>
        protected string[] GetCustomParameterArray(ObjectProperty property, params char[] delim) =>
            GetCustomParameterArray(GetObjectPropertyName(property), delim);

        [ExcludeFromCodeCoverage]
        internal string[] GetCustomParameterArray(ObjectPropertyInternal property, params char[] delim) =>
            GetCustomParameterArray(GetObjectPropertyName(property), delim);

        /// <summary>
        /// Retrieves the original value of a serialized array property from the underlying parameter set, using its raw serialized name.
        /// </summary>
        /// <param name="name">The raw name of the parameter</param>
        /// <param name="delim">The value that was used to combine the array when originally serialized via <see cref="SetCustomParameterArray(string, string[], char[])"/> </param>
        /// <returns>The original unserialized value.</returns>
        protected string[] GetCustomParameterArray(string name, params char[] delim)
        {
            if (delim == null)
                throw new ArgumentNullException(nameof(delim));

            if (delim.Length == 0)
                throw new ArgumentException("At least one element delimiter must be specified.", nameof(delim));

            var value = GetCustomParameterInternal(name);

            var array = value?.ToString().Split(delim, StringSplitOptions.RemoveEmptyEntries);

            return array;
        }

        #endregion

        /// <summary>
        /// Retrieves a parameter from the underlying parameter set using its raw, serialized name. If set does not contain a value for the specified parameter, null will be returned.
        /// </summary>
        /// <param name="name">The raw name of the parameter</param>
        /// <returns>The parameter's corresponding value. If the parameter was not found in the set, this value is null</returns>
        protected internal object GetCustomParameterInternal(string name)
        {
            var index = GetCustomParameterIndex(name);

            if (index == -1)
                return null;
            else
                return InternalParameters[index].Value;
        }

        #region SetCustomParameter

        /// <summary>
        /// Stores the value of a property in the underlying parameter set.
        /// </summary>
        /// <param name="property">The property whose value should be stored.</param>
        /// <param name="value">The value to serialize.</param>
        protected void SetCustomParameter(ObjectProperty property, object value) =>
            SetCustomParameterInternal(SetDependenciesAndGetPropertyName(property), value);

        [ExcludeFromCodeCoverage]
        internal void SetCustomParameter(ObjectPropertyInternal property, object value) =>
            SetCustomParameterInternal(SetDependenciesAndGetPropertyName(property), value);

        #endregion
        #region SetCustomParameterEnumXml

        /// <summary>
        /// Stores the serialized value of an enum property in the underlying parameter set.
        /// </summary>
        /// <typeparam name="T">The type of enum to serialize.</typeparam>
        /// <param name="property">The property whose value should be stored.</param>
        /// <param name="enum">The enum to serialize.</param>
        protected void SetCustomParameterEnumXml<T>(ObjectProperty property, T @enum) =>
            SetCustomParameterEnumXml(SetDependenciesAndGetPropertyName(property), @enum);

        [ExcludeFromCodeCoverage]
        internal void SetCustomParameterEnumXml<T>(ObjectPropertyInternal property, T @enum) =>
            SetCustomParameterEnumXml(SetDependenciesAndGetPropertyName(property), @enum);

        /// <summary>
        /// Stores the serialized value of an enum property in the underlying parameter set, using its raw serialized name.
        /// </summary>
        /// <typeparam name="T">The type of enum to serialize.</typeparam>
        /// <param name="name">The raw name of the parameter.</param>
        /// <param name="enum">The enum to serialize.</param>
        protected void SetCustomParameterEnumXml<T>(string name, T @enum) =>
            SetCustomParameterInternal(name, GetEnumVal(@enum));

        private object GetEnumVal<T>(T @enum)
        {
            if (@enum == null)
                return null;

            //If we're not an enum we expect to throw an exception
            return ((Enum)(object)@enum).EnumToXml();
        }

        #endregion
        #region SetCustomParameterBool

        /// <summary>
        /// Stores the serialized value of a boolean property in the underlying parameter set.
        /// </summary>
        /// <param name="property">The property whose value should be stored.</param>
        /// <param name="value">The bool to serialize.</param>
        protected void SetCustomParameterBool(ObjectProperty property, bool? value) =>
            SetCustomParameterBool(SetDependenciesAndGetPropertyName(property), value);

        [ExcludeFromCodeCoverage]
        internal void SetCustomParameterBool(ObjectPropertyInternal property, bool? value) =>
            SetCustomParameterBool(SetDependenciesAndGetPropertyName(property), value);

        /// <summary>
        /// Stores the serialized value of a boolean property in the underlying parameter set, using its raw serialized name.
        /// </summary>
        /// <param name="name">The raw name of the parameter.</param>
        /// <param name="value">The bool to serialize.</param>
        protected void SetCustomParameterBool(string name, bool? value) =>
            SetCustomParameterInternal(name, Convert.ToInt32(value));

        #endregion
        #region SetCustomParameterArray

        /// <summary>
        /// Stores the serialized value of an array property in the underlying parameter set.
        /// </summary>
        /// <param name="property">The property whose value should be stored.</param>
        /// <param name="value">The array to serialize.</param>
        /// <param name="delim">The character(s) that should delimit each array entry.</param>
        protected void SetCustomParameterArray(ObjectProperty property, string[] value, params char[] delim) =>
            SetCustomParameterArray(SetDependenciesAndGetPropertyName(property), value, delim);

        [ExcludeFromCodeCoverage]
        internal void SetCustomParameterArray(ObjectPropertyInternal property, string[] value, params char[] delim) =>
            SetCustomParameterArray(SetDependenciesAndGetPropertyName(property), value, delim);

        /// <summary>
        /// Stores the serialized value of an array property in the underlying parameter set, using its raw serialized name.
        /// </summary>
        /// <param name="name">The raw name of the parameter.</param>
        /// <param name="value">The array to serialize.</param>
        /// <param name="delim">The character(s) that should delimit each array entry.</param>
        protected void SetCustomParameterArray(string name, string[] value, params char[] delim)
        {
            if (delim == null)
                throw new ArgumentNullException(nameof(delim));

            if (delim.Length == 0)
                throw new ArgumentException("At least one element delimiter must be specified.", nameof(delim));

            var str = value != null ? string.Join(new string(delim), value) : null;

            SetCustomParameterInternal(name, str);
        }

        #endregion

        /// <summary>
        /// Stores the value of a property in the underlying set, using its raw serialized name.
        /// </summary>
        /// <param name="name">The raw name of the parameter.</param>
        /// <param name="value">The value to store.</param>
        protected void SetCustomParameterInternal(string name, object value)
        {
            value = PSObjectUtilities.CleanPSObject(value);

            var parameter = new CustomParameter(name, value);

            ((ICustomParameterContainer)this).AddParameter(parameter);
        }

        #region RemoveCustomParameter

        /// <summary>
        /// Removes a parameter from the underlying parameter set.
        /// </summary>
        /// <param name="property">The name of the parameter to remove.</param>
        /// <returns>True if the specified parameter was found and removed. Otherwise, false.</returns>
        protected bool RemoveCustomParameter(ObjectProperty property) =>
            RemoveCustomParameterInternal(GetObjectPropertyName(property));

        /// <summary>
        /// Removes a parameter from the underlying parameter set.
        /// </summary>
        /// <param name="property">The name of the parameter to remove.</param>
        /// <returns>True if the specified parameter was found and removed. Otherwise, false.</returns>
        internal bool RemoveCustomParameter(ObjectPropertyInternal property) =>
            RemoveCustomParameterInternal(GetObjectPropertyName(property));

        #endregion

        /// <summary>
        /// Removes a parameter from the underlying parameter set.
        /// </summary>
        /// <param name="name">The name of the parameter to remove.</param>
        /// <returns>True if the specified parameter was found and removed. Otherwise, false.</returns>
        protected bool RemoveCustomParameterInternal(string name)
        {
            var index = GetCustomParameterIndex(name);

            if (index == -1)
                return false;

            InternalParameters.RemoveAt(index);
            return true;
        }

        #region Helpers

        private string SetDependenciesAndGetPropertyName(Enum property)
        {
            var name = GetObjectPropertyName(property);

            if (constructorScope != null)
            {
                if (constructorScope.Disposed)
                    parser.AddDependents<ObjectProperty>(property);
            }
            else
            {
                //Don't set inherited properties when we first initialize the property
                //(which should be being done in the constructur)
                if (GetCustomParameterIndex(name) != -1)
                    parser.AddDependents<ObjectProperty>(property);
            }

            return name;
        }

        private string GetObjectPropertyName(Enum property, bool forceOriginalName = false)
        {
            var name = ObjectPropertyParser.GetObjectPropertyName(property);

            if (forceOriginalName)
                return name;

            string realName;

            if (property is ObjectProperty && nameOverrideMap.TryGetValue((ObjectProperty) property, out realName))
                name = realName;

            return name;
        }

        private int GetCustomParameterIndex(string name)
        {
            var index = InternalParameters.FindIndex(a => a.Name == name);

            return index;
        }

        #endregion

        void ICustomParameterContainer.AddParameter(CustomParameter parameter)
        {
            var index = GetCustomParameterIndex(parameter.Name);

            if (index == -1)
                InternalParameters.Add(parameter);
            else
                InternalParameters[index] = parameter;
        }

        ICollection<CustomParameter> ICustomParameterContainer.GetParameters() => InternalParameters;

        bool ICustomParameterContainer.AllowDuplicateParameters => true;

        PropertyCache IPropertyCacheResolver.GetPropertyCache(Enum property)
        {
            return ObjectPropertyParser.GetPropertyInfoViaTypeLookup(property);
        }

        internal List<CustomParameter> InternalParameters
        {
            get
            {
                if (this[Parameter.Custom] == null)
                    this[Parameter.Custom] = new List<CustomParameter>();

                var list = (List<CustomParameter>) this[Parameter.Custom];

                if (list.Any(v => v == null))
                {
                    list = list.Where(v => v != null).ToList();
                    this[Parameter.Custom] = list;
                }

                return list;
            }
        }

        /// <summary>
        /// Overrides the the name a typed property is stored as. If a parameter for the specified property already exists, it will be automatically updated to use the new name.
        /// </summary>
        /// <param name="property">The property to override the name of.</param>
        /// <param name="newName">The name to assign the property.</param>
        public void AddNameOverride(ObjectProperty property, string newName)
        {
            if (newName == null)
                throw new ArgumentNullException(nameof(newName));

            string currentName;

            if(!nameOverrideMap.TryGetValue(property, out currentName))
                currentName = GetObjectPropertyName(property, true);

            nameOverrideMap[property] = newName;

            var existingParameter = InternalParameters.FirstOrDefault(p => p.Name == currentName);

            if (existingParameter != null)
                existingParameter.Name = newName;
        }

        /// <summary>
        /// Retrieves all name overrides that are defined on this object.
        /// </summary>
        /// <returns>A read-only dictionary of overrides that are defined on this object.</returns>
        public IReadOnlyDictionary<ObjectProperty, string> GetNameOverrides() => new ReadOnlyDictionary<ObjectProperty, string>(nameOverrideMap);

        /// <summary>
        /// Determines whether this object contains an override for a specified property.
        /// </summary>
        /// <param name="property">The property to check for the existence of.</param>
        /// <returns>True if these parameters contain an override for the specified object property. Otherwise, false.</returns>
        public bool ContainsNameOverride(ObjectProperty property) => nameOverrideMap.ContainsKey(property);

        /// <summary>
        /// Removes a name override for a specified property. If a parameter for the property was successfully removed, the parameter will be reverted to use its original name.
        /// </summary>
        /// <param name="property">The property to remove the name override for.</param>
        /// <returns>If a name override for the specified property existed and was removed, true. Otherwise, false.</returns>
        public bool RemoveNameOverride(ObjectProperty property)
        {
            string currentName;

            if (nameOverrideMap.TryGetValue(property, out currentName) && nameOverrideMap.Remove(property))
            {
                var existingParameter = InternalParameters.FirstOrDefault(p => p.Name == currentName);

                existingParameter.Name = GetObjectPropertyName(property, true);

                return true;
            }

            return false;
        }
    }
}
