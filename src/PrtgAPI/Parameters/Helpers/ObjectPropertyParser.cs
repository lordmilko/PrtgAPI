using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters.Helpers
{
    internal class ObjectPropertyParser
    {
        ICustomParameterContainer container;
        IPropertyCacheResolver cacheResolver;
        Func<Enum, PropertyCache, string> nameResolver;

        public List<Enum> Properties { get; } = new List<Enum>();

        internal ObjectPropertyParser(
            ICustomParameterContainer container,
            IPropertyCacheResolver cacheResolver,
            Func<Enum, PropertyCache, string> nameResolver)
        {
            this.container = container;
            this.cacheResolver = cacheResolver;
            this.nameResolver = nameResolver;
        }

        internal void ParseValueAndDependents<T>(Enum property, object value, bool disableDependentsOnNotReqiuiredValue)
        {
            var cache = cacheResolver.GetPropertyCache(property);

            var parser = new DynamicPropertyTypeParser(property, cache, value);

            AddValue(parser);
            AddDependents<T>(parser, disableDependentsOnNotReqiuiredValue);
        }

        internal void AddDependents<T>(Enum property)
        {
            AddDependentPropertyRecursiveUp<T>(property);
        }

        void AddDependents<T>(DynamicPropertyTypeParser parser, bool disableDependentsOnNotReqiuiredValue)
        {
            var childrenOfParent = parser.Property.GetDependentProperties();

            bool isParent = childrenOfParent.Any();

            AddDependentPropertyRecursiveUp<T>(parser.Property);

            if (isParent)
            {
                TryDisableDependentProperties(parser, disableDependentsOnNotReqiuiredValue, childrenOfParent);
            }
        }

        internal void AddDependentProperty(object val, Enum parent)
        {
            var cache = cacheResolver.GetPropertyCache(parent);

            var dependencyParser = new DynamicPropertyTypeParser(parent, cache, val);
            var parameter = dependencyParser.GetParameter(nameResolver);

            //If there is already an existing value (i.e. from the parameter being specified explicitly) don't set the dependency
            if (!ParameterExists(parameter))
                container.AddParameter(parameter);
        }

        private bool ParameterExists(CustomParameter parameter)
        {
            if (container.AllowDuplicateParameters)
                return false;

            var existingParameters = container.GetParameters();

            return existingParameters.Any(p => p.Name == parameter.Name);
        }

        private void AddDependentPropertyRecursiveUp<T>(Enum property)
        {
            Properties.Add(property);

            //Given a property, get that objects parent, and then that objects parent, and so on

            var parentOfChild = property.GetEnumAttributes<DependentPropertyAttribute>();

            bool isChild = parentOfChild != null;

            if (isChild)
            {
                foreach (var attrib in parentOfChild)
                {
                    var parent = attrib.Property;

                    AddDependentProperty(attrib.RequiredValue, parent);
                    AddDependentPropertyRecursiveUp<T>(parent);
                }
            }
        }

        private void TryDisableDependentProperties(DynamicPropertyTypeParser parser, bool disableDependentsOnNotReqiuiredValue, Enum[] childrenOfParent)
        {
            //If we reach this method, we know that our property is a parent

            if (parser != null)
                TryDisableDependentPropertiesWithTypeCheck(parser, disableDependentsOnNotReqiuiredValue, childrenOfParent);
            else
                TryDisableDependentPropertiesWithoutTypeCheck(childrenOfParent);
        }

        private void TryDisableDependentPropertiesWithTypeCheck(DynamicPropertyTypeParser parser, bool disableDependentsOnNotReqiuiredValue, Enum[] childrenOfParent)
        {
            var parentValueAsTypeRequiredByProperty = parser.ToPrimitivePropertyType();

            foreach (var child in childrenOfParent)
            {
                var attrib = child.GetEnumAttribute<DependentPropertyAttribute>(); //By virtue of the fact we're a child, we can guarantee we have a DependentPropertyAttribute
                var propertyRequiredValue = attrib.RequiredValue;

                if (propertyRequiredValue != null && propertyRequiredValue.GetType() != parentValueAsTypeRequiredByProperty.GetType())
                    throw new InvalidTypeException($"Dependencies of property '{parser.Property}' should be of type {parser.PropertyType}, however property '{child}' is dependent on type '{propertyRequiredValue.GetType()}'.");

                if (parentValueAsTypeRequiredByProperty.Equals(propertyRequiredValue)) // == does not work since we are potentially comparing value types wrapped as type object
                {
                    //If a child has a reverse dependency (e.g. VerticalAxisMin must be included when VerticalAxisScaling == Manual)
                    //Then we should include those children, where they will be set to empty causing PRTG to throw an exception
                    if (attrib.ReverseDependency)
                    {
                        AddParameterIfNotSet(child, cacheResolver.GetPropertyCache(child), string.Empty);
                        var newChildren = child.GetDependentProperties();
                        TryDisableDependentProperties(null, disableDependentsOnNotReqiuiredValue, newChildren);
                    }
                }
                else
                {
                    //If we wish to disable dependents when their parent value does not match their required value, add and assign them an empty value
                    if (disableDependentsOnNotReqiuiredValue)
                    {
                        AddParameterIfNotSet(child, cacheResolver.GetPropertyCache(child), string.Empty);
                        var newChildren = child.GetDependentProperties();
                        TryDisableDependentProperties(null, disableDependentsOnNotReqiuiredValue, newChildren);
                    }
                }
            }
        }

        private void TryDisableDependentPropertiesWithoutTypeCheck(Enum[] childrenOfParent)
        {
            foreach (var child in childrenOfParent)
            {
                var attrib = child.GetEnumAttribute<DependentPropertyAttribute>();

                if (attrib.ReverseDependency)
                {
                    AddParameterIfNotSet(child, cacheResolver.GetPropertyCache(child), string.Empty);
                }
            }
        }

        void AddValue(DynamicPropertyTypeParser parser)
        {
            var parameter = parser.GetParameter(nameResolver);
            container.AddParameter(parameter);

            var secondaryPropertyAttrib = parser.Property.GetEnumAttribute<SecondaryPropertyAttribute>();

            if (secondaryPropertyAttrib != null)
            {
                var secondaryPropertyName = $"{secondaryPropertyAttrib.Property.ToString().ToLower()}_";

                switch (secondaryPropertyAttrib.Strategy)
                {
                    case SecondaryPropertyStrategy.MultipleSerializable:
                        if (parser.Value is IMultipleSerializable)
                        {
                            var val2 = ((IMultipleSerializable)parser.Value).GetSerializedFormats().Last();

                            container.AddParameter(new CustomParameter(secondaryPropertyName, val2));
                        }
                        break;

                    case SecondaryPropertyStrategy.SameValue:
                        container.AddParameter(new CustomParameter(secondaryPropertyName, parameter.Value));
                        break;

                    default:
                        throw new NotImplementedException($"Don't know how to handle {nameof(SecondaryPropertyStrategy)} '{secondaryPropertyAttrib.Strategy}'.");
                }
            }
        }

        void AddParameterIfNotSet(Enum property, PropertyCache cache, object value)
        {
            var parameter = new CustomParameter(GetParameterName(property, cache), value?.ToString());

            if (!ParameterExists(parameter))
                container.AddParameter(parameter);
        }

        #region Name Resolution

        private string GetParameterName(Enum property, PropertyCache cache)
        {
            if (nameResolver != null)
                return nameResolver(property, cache);

            return GetObjectPropertyNameViaCache(property, cache);
        }

        internal static string GetObjectPropertyName(ObjectProperty property) => GetObjectPropertyName((Enum)property);

        internal static string GetObjectPropertyName(Enum property)
        {
            var cache = GetPropertyInfoViaTypeLookup(property, property is ObjectPropertyInternal);

            return GetObjectPropertyNameViaCache(property, cache);
        }

        internal static string GetObjectPropertyNameViaCache(Enum property, PropertyCache cache)
        {
            var mergeable = property.GetEnumAttribute<MergeableAttribute>();

            if (mergeable != null)
                throw new InvalidOperationException($"'{property}' is a virtual property and cannot be retrieved directly. To access this value, property '{mergeable.Dependency}' should be retrieved instead.");

            var attribute = cache?.GetAttribute<XmlElementAttribute>();
            string name;

            if (attribute == null)
            {
                var description = property.GetDescription(false);

                if (description == null)
                    throw new MissingAttributeException(property.GetType(), property.ToString(), typeof(DescriptionAttribute));

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

        #endregion
        #region Property Cache     

        internal static PropertyCache GetPropertyInfoViaTypeLookup(Enum property, bool allowNoProperty = false)
        {
            var attr = property.GetEnumAttribute<TypeLookupAttribute>(!allowNoProperty);

            if (attr == null)
                return null;

            var prop = attr.Class.GetTypeCache().Properties.FirstOrDefault(p => p.Property.Name == property.ToString());

            if (prop == null)
                throw new MissingMemberException($"Property {property} cannot be found on type {attr.Class} pointed to by {nameof(TypeLookupAttribute)}.");

            return prop;
        }

        internal static PropertyCache GetPropertyInfoViaPropertyParameter<TObject>(Enum property)
        {
            var prop = typeof(TObject).GetTypeCache().Properties.FirstOrDefault(p => p.GetAttribute<PropertyParameterAttribute>()?.Property.Equals(property) == true);

            if (prop == null)
                throw new MissingAttributeException(typeof(TObject), property.ToString(), typeof(PropertyParameterAttribute));

            return prop;
        }

        #endregion
    }
}
