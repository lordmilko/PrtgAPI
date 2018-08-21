using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell;

namespace PrtgAPI.Dynamic
{
    class ParameterProxy : DynamicProxy<IDynamicParameters>
    {
        public override bool TryGetMember(IDynamicParameters instance, GetMemberBinder binder, out object value)
        {
            lock (lockObject)
            {
                value = instance[binder.Name];

                return true;
            }
        }

        public override bool TrySetMember(IDynamicParameters instance, SetMemberBinder binder, object value)
        {
            lock (lockObject)
            {
                instance[binder.Name] = value;

                return true;
            }
        }

        [ExcludeFromCodeCoverage]
        public override IEnumerable<string> GetDynamicMemberNames(IDynamicParameters instance)
        {
            lock (lockObject)
            {
                var customParameters = instance[Parameter.Custom] as List<CustomParameter>;

                if (customParameters != null)
                    return customParameters.Where(p => p.Value is PSVariableEx).Select(p => ((PSVariableEx)p.Value).Name);

                return base.GetDynamicMemberNames(instance);
            }
        }
    }
}
