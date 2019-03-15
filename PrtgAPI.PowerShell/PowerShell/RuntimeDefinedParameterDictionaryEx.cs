using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;

namespace PrtgAPI.PowerShell
{
    class RuntimeDefinedParameterDictionaryEx : RuntimeDefinedParameterDictionary
    {
        [ExcludeFromCodeCoverage]
        public new void Add(string key, RuntimeDefinedParameter value)
        {
            Debug.Assert(false, $"Cannot call method '{nameof(Add)}' on {nameof(RuntimeDefinedParameterDictionaryEx)}. Use '{nameof(AddOrMerge)}' instead");
            throw new NotSupportedException();
        }

        public void AddOrMerge(string name, Type parameterType, params Attribute[] attributes)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (parameterType == null)
                throw new ArgumentNullException(nameof(parameterType));

            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            if (attributes.OfType<ParameterAttribute>().Count() == 0)
                throw new ArgumentException($"At least one {nameof(ParameterAttribute)} must be specified.");

            RuntimeDefinedParameter existingValue;

            Debug.Assert(attributes.OfType<ParameterAttribute>().All(p => p.ParameterSetName != ParameterAttribute.AllParameterSets), "A parameter was specified in the default parameter set");

            if (TryGetValue(name, out existingValue))
            {
                Debug.Assert(existingValue.ParameterType == parameterType, $"Found two parameters with the same name ({name}) but different types: '{parameterType}' and {existingValue.ParameterType}");

                //If this isn't true in a release build, we just won't support the parameter
                if (existingValue.ParameterType == parameterType)
                {
                    //Just add the additional parameters for the new parameter sets
                    foreach (var attribute in attributes)
                    {
                        var parameterAttribute = attribute as ParameterAttribute;

                        if (parameterAttribute != null)
                        {
                            //If a ParameterAttribute has already been added for a given parameter set before (e.g. "Device" which is in all parameter sets), skip it
                            if (!existingValue.Attributes.OfType<ParameterAttribute>().Any(p => p.ParameterSetName == parameterAttribute.ParameterSetName))
                                existingValue.Attributes.Add(attribute);
                        }
                        else
                        {
                            existingValue.Attributes.Add(attribute);
                        }
                    }
                }
            }
            else
            {
                //Just add the new parameter
                base.Add(name, new RuntimeDefinedParameterEx(name, parameterType, attributes));
            }
        }
    }
}
