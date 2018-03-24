using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.Helpers;

namespace PrtgAPI.PowerShell
{
    class DynamicParameterSet<T> where T : struct
    {
        public RuntimeDefinedParameterDictionary Parameters { get; set; } = new RuntimeDefinedParameterDictionary();

        public string[] ParameterSetNames { get; }

        public DynamicParameterSet(string parameterSet, Func<T, PropertyInfo> getPropertyInfo) : this(new[] {parameterSet}, getPropertyInfo)
        {
        }

        public DynamicParameterSet(string[] parameterSets, Func<T, PropertyInfo> getPropertyInfo)
        {
            ParameterSetNames = parameterSets;

            var values = Enum.GetValues(typeof(T));

            foreach (T value in values)
            {
                var info = getPropertyInfo(value);

                AddParameter(value.ToString(), info.PropertyType);
            }
        }

        private void AddParameter(string name, Type type, bool mandatory = false, bool valueFromPipeline = false)
        {
            var attributeCollection = new Collection<Attribute>();

            foreach (var set in ParameterSetNames)
            {
                var attribute = new ParameterAttribute { ParameterSetName = set };

                if (mandatory)
                    attribute.Mandatory = true;

                if (valueFromPipeline)
                    attribute.ValueFromPipeline = true;

                attributeCollection.Add(attribute);
            }

            Parameters.Add(name, new RuntimeDefinedParameter(name, Nullable.GetUnderlyingType(type) ?? type, attributeCollection));
        }

        public List<TParam> GetBoundParameters<TParam>(PSCmdlet cmdlet, Func<T, object, TParam> createParam)
        {
            var matches = cmdlet.MyInvocation.BoundParameters
                .Where(k => Parameters.Any(p => p.Key == k.Key))
                .Select(p => createParam(p.Key.ToEnum<T>(), p.Value)).ToList();

            return matches;
        }
    }
}
