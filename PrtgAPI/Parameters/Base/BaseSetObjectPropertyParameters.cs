using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Helpers;
using PrtgAPI.Request.Serialization.Cache;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Allows specifying an internal enum type when derived from <see cref="BaseSetObjectPropertyParameters{TObjectProperty}"/>.
    /// </summary>
    /// <typeparam name="TObjectPropertyInternal">The type of internal property to use.</typeparam>
    interface IObjectInternalProperty<TObjectPropertyInternal>
    {
    }

    abstract class BaseSetObjectPropertyParameters<TObjectProperty> : BaseParameters, IMultiTargetParameters, IHtmlParameters
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.EditSettings;

        private bool paramsInitialized;

        public List<CustomParameter> CustomParameters
        {
            get { return (List<CustomParameter>)this[Parameter.Custom]; }
            set { this[Parameter.Custom] = value; }
        }

        protected void AddTypeSafeValue(Enum property, object value, bool disableDependentsOnNotReqiuiredValue)
        {
            if (!paramsInitialized)
            {
                CustomParameters = new List<CustomParameter>();
                paramsInitialized = true;
            }

            var cache = GetPropertyCache(property);

            var parser = new DynamicPropertyTypeParser(property, cache, value);

            AddValue(parser);
            AddDependents(parser, disableDependentsOnNotReqiuiredValue);
        }

        void AddValue(DynamicPropertyTypeParser parser)
        {
            var val = parser.ParseValue();

            AddParameter(parser.Property, parser.Cache, val);

            var secondaryPropertyAttrib = parser.Property.GetEnumAttribute<SecondaryPropertyAttribute>();

            if (secondaryPropertyAttrib != null)
            {
                if (parser.Value is IMultipleSerializable)
                {
                    var val2 = ((IMultipleSerializable)parser.Value).GetSerializedFormats().Last();

                    CustomParameters.Add(new CustomParameter($"{secondaryPropertyAttrib.Name}_", val2));
                }
            }
        }

        void AddDependents(DynamicPropertyTypeParser parser, bool disableDependentsOnNotReqiuiredValue)
        {
            var childrenOfParent = parser.Property.GetDependentProperties<TObjectProperty>().Cast<Enum>().ToList();

            bool isParent = childrenOfParent.Any();

            AddDependentPropertyRecursiveUp(parser.Property);

            if (isParent)
            {
                TryDisableDependentProperties(parser, disableDependentsOnNotReqiuiredValue, childrenOfParent);
            }
        }

        internal void AddDependentProperty(object val, Enum parent)
        {
            var cache = GetPropertyCache(parent);

            var dependencyParser = new DynamicPropertyTypeParser(parent, cache, val);
            AddParameter(parent, cache, dependencyParser.ParseValue());
        }

        private void AddDependentPropertyRecursiveUp(Enum property)
        {
            //Given a property, get that objects parent, and then that objects parent, and so on

            var parentOfChild = property.GetEnumAttributes<DependentPropertyAttribute>();

            bool isChild = parentOfChild != null;

            if (isChild)
            {
                foreach (var attrib in parentOfChild)
                {
                    Enum parent;

                    try
                    {
                        parent = (Enum)(object)attrib.Name.ToEnum<TObjectProperty>();
                    }
                    catch (ArgumentException)
                    {
                        var @interface = GetType().GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IObjectInternalProperty<>));

                        if (@interface != null)
                        {
                            var internalType = @interface.GetGenericArguments().First();

                            parent = (Enum)Enum.Parse(internalType, attrib.Name, true);
                        }
                        else
                            throw;
                    }

                    AddDependentProperty(attrib.RequiredValue, parent);
                    AddDependentPropertyRecursiveUp(parent);
                }
            }
        }

        private void TryDisableDependentProperties(DynamicPropertyTypeParser parser, bool disableDependentsOnNotReqiuiredValue, List<Enum> childrenOfParent)
        {
            //If we reach this method, we know that our property is a parent

            if (parser != null)
                TryDisableDependentPropertiesWithTypeCheck(parser, disableDependentsOnNotReqiuiredValue, childrenOfParent);
            else
                TryDisableDependentPropertiesWithoutTypeCheck(childrenOfParent);
        }

        private void TryDisableDependentPropertiesWithTypeCheck(DynamicPropertyTypeParser parser, bool disableDependentsOnNotReqiuiredValue, List<Enum> childrenOfParent)
        {
            var parentValueAsTypeRequiredByProperty = parser.ToPrimitivePropertyType();

            foreach (var child in childrenOfParent)
            {
                var attrib = child.GetEnumAttribute<DependentPropertyAttribute>(); //By virtue of the fact we're a child, we can guarantee we have a DependentPropertyAttribute
                var propertyRequiredValue = attrib.RequiredValue;

                if (propertyRequiredValue != null && propertyRequiredValue.GetType() != parentValueAsTypeRequiredByProperty.GetType())
                    throw new InvalidTypeException($"Dependencies of property '{parser.Property}' should be of type {parser.PropertyType}, however property '{child}' is dependent on type '{propertyRequiredValue.GetType()}'");

                if (parentValueAsTypeRequiredByProperty.Equals(propertyRequiredValue)) // == does not work since we are potentially comparing value types wrapped as type object
                {
                    //If a child has a reverse dependency (e.g. VerticalAxisMin must be included when VerticalAxisScaling = Manual)
                    //Then we should include those children, where they will be set to empty causing PRTG to throw an exception
                    if (attrib.ReverseDependency)
                    {
                        AddParameter(child, GetPropertyCache(child), string.Empty);
                        var newChildren = child.GetDependentProperties<TObjectProperty>().Cast<Enum>().ToList();
                        TryDisableDependentProperties(null, disableDependentsOnNotReqiuiredValue, newChildren);
                    }
                }
                else
                {
                    //If we wish to disable dependents when their parent value does not match their required value, add and assign them an empty value
                    if (disableDependentsOnNotReqiuiredValue)
                    {
                        AddParameter(child, GetPropertyCache(child), string.Empty);
                        var newChildren = child.GetDependentProperties<TObjectProperty>().Cast<Enum>().ToList();
                        TryDisableDependentProperties(null, disableDependentsOnNotReqiuiredValue, newChildren);
                    }
                }
            }
        }

        private void TryDisableDependentPropertiesWithoutTypeCheck(List<Enum> childrenOfParent)
        {
            foreach (var child in childrenOfParent)
            {
                var attrib = child.GetEnumAttribute<DependentPropertyAttribute>();

                if (attrib.ReverseDependency)
                {
                    AddParameter(child, GetPropertyCache(child), string.Empty);
                }
            }
        }

        void AddParameter(Enum property, PropertyCache cache, object value)
        {
            CustomParameters.Add(new CustomParameter(GetParameterName(property, cache), value?.ToString()));
        }

        protected abstract PropertyCache GetPropertyCache(Enum property);

        internal static PropertyCache GetPropertyInfoViaTypeLookup(Enum property)
        {
            var attr = property.GetEnumAttribute<TypeLookupAttribute>(true);
            var prop = attr.Class.GetTypeCache().Cache.Properties.FirstOrDefault(p => p.Property.Name == property.ToString());

            if (prop == null)
                throw new MissingMemberException($"Property {property} cannot be found on type {attr.Class} pointed to by {nameof(TypeLookupAttribute)}");

            return prop;
        }

        internal static PropertyCache GetPropertyInfoViaPropertyParameter<T>(Enum property)
        {
            var prop = typeof(T).GetTypeCache().Cache.Properties.FirstOrDefault(p => p.GetAttribute<PropertyParameterAttribute>()?.Name == property.ToString());

            if (prop == null)
                throw new MissingAttributeException(typeof(T), property.ToString(), typeof(PropertyParameterAttribute));

            return prop;
        }

        protected virtual string GetParameterName(Enum property, PropertyCache cache)
        {
            return GetParameterNameStatic(property, cache);
        }

        internal static string GetParameterName(ObjectProperty property)
        {
            var cache = BaseSetObjectPropertyParameters<ObjectProperty>.GetPropertyInfoViaTypeLookup(property);

            return GetParameterNameStatic(property, cache);
        }

        internal static string GetParameterNameStatic(Enum property, PropertyCache cache)
        {
            var attribute = cache.GetAttribute<XmlElementAttribute>();
            string name;

            if (attribute == null)
            {
                var description = property.GetDescription(false);

                if (description == null)
                    throw new MissingAttributeException(typeof(TObjectProperty), property.ToString(), typeof(DescriptionAttribute));

                name = description;
            }
            else
            {
                name = attribute.ElementName.Substring("injected_".Length);
            }

            if (property.GetEnumAttribute<LiteralValueAttribute>() == null)
                name += "_";

            return name;
        }

        /// <summary>
        /// Searches this object's <see cref="CustomParameters"/> for any objects with a duplicate <see cref="CustomParameter.Name"/> and removes all but the last one.
        /// </summary>
        internal void RemoveDuplicateParameters()
        {
            var groups = CustomParameters.GroupBy(p => p.Name).Where(g => g.Count() > 1);

            foreach (var group in groups)
            {
                var list = group.ToList();

                for (int i = 0; i < list.Count - 1; i++)
                {
                    CustomParameters.Remove(list[i]);
                }
            }
        }

        int[] IMultiTargetParameters.ObjectIds
        {
            get { return ObjectIdsInternal; }
            set { ObjectIdsInternal = value; }
        }

        protected abstract int[] ObjectIdsInternal { get; set; }
    }
}
