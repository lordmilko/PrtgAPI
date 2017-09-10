using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Helpers;
using PrtgAPI.Parameters.Helpers;

namespace PrtgAPI.Parameters
{
    abstract class BaseSetObjectPropertyParameters<TObjectProperty> : Parameters
    {
        public List<CustomParameter> CustomParameters
        {
            get { return (List<CustomParameter>)this[Parameter.Custom]; }
            set { this[Parameter.Custom] = value; }
        }

        protected void AddTypeSafeValue(Enum property, object value, bool disableDependentsOnFalse)
        {
            CustomParameters = new List<CustomParameter>();

            var info = GetPropertyInfo(property);

            var parser = new DynamicPropertyTypeParser(property, info, value);

            AddValue(parser);
            AddDependents(parser, disableDependentsOnFalse);
        }

        void AddValue(DynamicPropertyTypeParser parser)
        {
            var val = parser.ParseValue();

            AddParameter(parser.Property, parser.Info, val);
        }

        

        void AddDependents(DynamicPropertyTypeParser parser, bool disableDependentsOnFalse)
        {
            var parentOfChild = parser.Property.GetEnumAttribute<DependentPropertyAttribute>();
            var childrenOfParent = parser.Property.GetDependentProperties<TObjectProperty>().Cast<Enum>().ToList();

            bool isChild = parentOfChild != null;
            bool isParent = childrenOfParent.Any();

            if (isChild && isParent)
                throw new NotSupportedException($"Property {parser.Property} is the child of property {parentOfChild} and parent of properties {string.Join(", ", childrenOfParent)} however multi-level relationships are not supported.");

            if (isChild) //No need to worry about disabling things, since children can't also be parents
            {
                AddDependentProperty(parentOfChild);
            }

            if (isParent) //Parents can't be children
            {
                //We don't require any children be set when a parent is modified, only potentially that they be unset (when the parent is disabled)
                TryDisableDependentProperties(parser, disableDependentsOnFalse, childrenOfParent);
            }
        }

        private void AddDependentProperty(DependentPropertyAttribute attrib)
        {
            var dependentProperty = (Enum)(object)attrib.Name.ToEnum<TObjectProperty>();
            var info = GetPropertyInfo(dependentProperty);

            var dependencyParser = new DynamicPropertyTypeParser(dependentProperty, info, attrib.RequiredValue);
            AddParameter(dependentProperty, info, dependencyParser.ParseValue());
        }

        private void TryDisableDependentProperties(DynamicPropertyTypeParser parser, bool disableDependentsOnFalse, List<Enum> childrenOfParent)
        {
            if (disableDependentsOnFalse)
            {
                var asPropertyType = parser.ToPrimitivePropertyType();

                var diff = childrenOfParent.Where(child =>
                {
                    var attrib = child.GetEnumAttribute<DependentPropertyAttribute>();
                    var val = attrib?.RequiredValue;

                    if (val != null && val.GetType() != asPropertyType.GetType())
                        throw new InvalidTypeException($"Dependencies of property '{parser.Property}' should be of type {parser.PropertyType}, however property '{child}' is dependent on type '{val.GetType()}'");

                    //If the specified value is equal to the RequiredValue (e.g. LimitsEnabled = true) we don't need to modify any of the children
                    //(e.g. UpperErrorLimit, which has a dependency on LimitsEnabled = true
                    if (asPropertyType.Equals(val)) // == does not work since we are potentially comparing value types wrapped as type object
                    {
                        //However, completely separately to this, if a child has a reverse dependency (e.g. VerticalAxisMin must be included when VerticalAxisScaling = Manual)
                        //Then we should include those children, where they will be set to empty causing PRTG to throw an exception
                        if (attrib?.ReverseDependency == true)
                            return true;

                        //Indicate the child does not need to be set as originally planned
                        return false;
                    }
                        

                    return true;
                }).ToList();

                foreach (var property in diff)
                {
                    AddParameter(property, GetPropertyInfo(property), string.Empty);
                }
            }
        }

        void AddParameter(Enum property, PropertyInfo info, object value)
        {
            CustomParameters.Add(new CustomParameter(GetParameterName((TObjectProperty)(object)property, info), HttpUtility.UrlEncode(value?.ToString())));
        }

        protected abstract PropertyInfo GetPropertyInfo(Enum property);

        internal static PropertyInfo GetPropertyInfoViaTypeLookup(Enum property)
        {
            var attr = (property).GetEnumAttribute<TypeLookupAttribute>(true);
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

        protected virtual string GetParameterName(TObjectProperty property, PropertyInfo info)
        {
            return GetParameterNameStatic(property, info);
        }

        internal static string GetParameterNameStatic(TObjectProperty property, PropertyInfo info)
        {
            var attribute = info.GetCustomAttribute<XmlElementAttribute>();
            string name;

            if (attribute == null)
            {
                var description = ((Enum)(object)property).GetDescription(false);

                if (description == null)
                    throw new MissingAttributeException(typeof(TObjectProperty), property.ToString(), typeof(DescriptionAttribute));

                name = description;
            }
            else
            {
                name = attribute.ElementName.Substring("injected_".Length);
            }

            if (((Enum)(object)property).GetEnumAttribute<LiteralValueAttribute>() == null)
                name += "_";

            return name;
        }
    }
}
