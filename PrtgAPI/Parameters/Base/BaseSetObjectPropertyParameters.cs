using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Helpers;
using PrtgAPI.Parameters.Helpers;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Allows specifying an internal enum type when derived from <see cref="BaseSetObjectPropertyParameters{TObjectProperty}"/>.
    /// </summary>
    /// <typeparam name="TObjectPropertyInternal">The type of internal property to use.</typeparam>
    interface IObjectInternalProperty<TObjectPropertyInternal>
    {
    }

    abstract class BaseSetObjectPropertyParameters<TObjectProperty> : Parameters
    {
        public List<CustomParameter> CustomParameters
        {
            get { return (List<CustomParameter>)this[Parameter.Custom]; }
            set { this[Parameter.Custom] = value; }
        }

        protected void AddTypeSafeValue(Enum property, object value, bool disableDependentsOnNotReqiuiredValue)
        {
            CustomParameters = new List<CustomParameter>();

            var info = GetPropertyInfo(property);

            var parser = new DynamicPropertyTypeParser(property, info, value);

            AddValue(parser);
            AddDependents(parser, disableDependentsOnNotReqiuiredValue);
        }

        void AddValue(DynamicPropertyTypeParser parser)
        {
            var val = parser.ParseValue();

            AddParameter(parser.Property, parser.Info, val);

            var secondaryPropertyAttrib = parser.Property.GetEnumAttribute<SecondaryPropertyAttribute>();

            if (secondaryPropertyAttrib != null)
            {
                if (parser.Value is IFormattableMultiple)
                {
                    var val2 = ((IFormattableMultiple)parser.Value).GetSerializedFormats().Last();

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

        private void AddDependentProperty(DependentPropertyAttribute attrib, Enum parent)
        {
            var info = GetPropertyInfo(parent);

            var dependencyParser = new DynamicPropertyTypeParser(parent, info, attrib.RequiredValue);
            AddParameter(parent, info, dependencyParser.ParseValue());
        }

        private void AddDependentPropertyRecursiveUp(Enum property)
        {
            //Given a property, get that objects parent, and then that objects parent, and so on

            var parentOfChild = property.GetEnumAttribute<DependentPropertyAttribute>();

            bool isChild = parentOfChild != null;

            if (isChild)
            {
                Enum parent;

                try
                {
                    parent = (Enum)(object)parentOfChild.Name.ToEnum<TObjectProperty>();
                }
                catch (ArgumentException)
                {
                    var @interface = GetType().GetInterfaces().FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IObjectInternalProperty<>));

                    if (@interface != null)
                    {
                        var internalType = @interface.GetGenericArguments().First();

                        parent = (Enum) Enum.Parse(internalType, parentOfChild.Name, true);
                    }
                    else
                        throw;
                }
                
                AddDependentProperty(parentOfChild, parent);
                AddDependentPropertyRecursiveUp(parent);
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
                        AddParameter(child, GetPropertyInfo(child), string.Empty);
                        var newChildren = child.GetDependentProperties<TObjectProperty>().Cast<Enum>().ToList();
                        TryDisableDependentProperties(null, disableDependentsOnNotReqiuiredValue, newChildren);
                    }
                }
                else
                {
                    //If we wish to disable dependents when their parent value does not match their required value, add and assign them an empty value
                    if (disableDependentsOnNotReqiuiredValue)
                    {
                        AddParameter(child, GetPropertyInfo(child), string.Empty);
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
                    AddParameter(child, GetPropertyInfo(child), string.Empty);
                }
            }
        }

        void AddParameter(Enum property, PropertyInfo info, object value)
        {
            CustomParameters.Add(new CustomParameter(GetParameterName(property, info), value?.ToString()));
        }

        protected abstract PropertyInfo GetPropertyInfo(Enum property);

        internal static PropertyInfo GetPropertyInfoViaTypeLookup(Enum property)
        {
            var attr = property.GetEnumAttribute<TypeLookupAttribute>(true);
            var prop = attr.Class.GetProperties().FirstOrDefault(p => p.Name == property.ToString());

            if (prop == null)
                throw new MissingMemberException($"Property {property} cannot be found on type {attr.Class} pointed to by {nameof(TypeLookupAttribute)}");

            return prop;
        }

        internal static PropertyInfo GetPropertyInfoViaPropertyParameter<T>(Enum property)
        {
            var prop = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<PropertyParameterAttribute>()?.Name == property.ToString());

            if (prop == null)
                throw new MissingAttributeException(typeof(T), property.ToString(), typeof(PropertyParameterAttribute));

            return prop;
        }

        protected virtual string GetParameterName(Enum property, PropertyInfo info)
        {
            return GetParameterNameStatic(property, info);
        }

        internal static string GetParameterName(ObjectProperty property)
        {
            var info = BaseSetObjectPropertyParameters<ObjectProperty>.GetPropertyInfoViaTypeLookup(property);

            return GetParameterNameStatic(property, info);
        }

        internal static string GetParameterNameStatic(Enum property, PropertyInfo info)
        {
            var attribute = info.GetCustomAttribute<XmlElementAttribute>();
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
    }
}
