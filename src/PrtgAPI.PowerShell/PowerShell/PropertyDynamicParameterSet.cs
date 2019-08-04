using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// Represents an object used for constructing dynamic parameters from objects whose properties correspond to values defined in an enum type.
    /// </summary>
    /// <typeparam name="T">The type of enum that corresponds to the object's properties.</typeparam>
    class PropertyDynamicParameterSet<T> where T : struct
    {
        public RuntimeDefinedParameterDictionary Parameters { get; set; } = new RuntimeDefinedParameterDictionary();

        public string[] ParameterSetNames { get; }

        private Cmdlet excludeStaticParameters;

        private string[] staticParameters;

        public PropertyDynamicParameterSet(string parameterSet, Func<T, Type> getPropertyType, Cmdlet excludeStaticParameters = null) : this(new[] {parameterSet}, getPropertyType, excludeStaticParameters)
        {
        }

        public PropertyDynamicParameterSet(string[] parameterSets, Func<T, Type> getPropertyType, Cmdlet excludeStaticParameters = null)
        {
            ParameterSetNames = parameterSets;
            this.excludeStaticParameters = excludeStaticParameters;

            var values = Enum.GetValues(typeof(T));

            foreach (T value in values)
            {
                var type = getPropertyType(value);

                if (type != null)
                    AddParameter(value.ToString(), type);
            }
        }

        private void AddParameter(string name, Type type, bool mandatory = false, bool valueFromPipeline = false)
        {
            var attributeCollection = new Collection<Attribute>();

            if (excludeStaticParameters != null)
            {
                if (staticParameters == null)
                    GetStaticParameters();

                if (staticParameters.Contains(name))
                    return;
            }

            foreach (var set in ParameterSetNames)
            {
                var attribute = new ParameterAttribute { ParameterSetName = set };

                if (mandatory)
                    attribute.Mandatory = true;

                if (valueFromPipeline)
                    attribute.ValueFromPipeline = true;

                attributeCollection.Add(attribute);
            }

            Parameters.Add(name, new RuntimeDefinedParameter(name, type.GetUnderlyingType(), attributeCollection));
        }

        private void GetStaticParameters()
        {
            staticParameters = ReflectionCacheManager.Get(excludeStaticParameters.GetType()).Properties
                .Where(p => p.GetAttribute<ParameterAttribute>() != null)
                .Select(p => p.Property.Name).ToArray();
        }

        public List<TParam> GetBoundParameters<TParam>(PSCmdlet cmdlet, Func<T, object, TParam> createParam)
        {
            Func<T, object, IEnumerable<TParam>> f = (a, b) => new List<TParam>
            {
                createParam(a, b)
            };

            return GetBoundParameters(cmdlet, f);
        }

        public List<TParam> GetBoundParameters<TParam>(PSCmdlet cmdlet, Func<T, object, IEnumerable<TParam>> createParam)
        {
            var matches = cmdlet.MyInvocation.BoundParameters
                .Where(k => Parameters.Any(p => p.Key == k.Key))
                .SelectMany(p => createParam(p.Key.ToEnum<T>(), p.Value)).ToList();

            return matches;
        }
    }
}
