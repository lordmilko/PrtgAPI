using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;

namespace PrtgAPI.Utilities
{
    class PSCmdletInvoker
    {
        public IPSCmdletEx Cmdlet { get; }

        public List<object> Output => ((DummyRuntime)((PSCmdlet)Cmdlet).CommandRuntime).Output;

        private Action<dynamic, Dictionary<string, object>> valueFromPipeline;

        private Lazy<string> parameterSetName;

        private PSCmdlet owner;

        public PSCmdletInvoker(PrtgCmdlet owner, IPSCmdletEx cmdlet, Lazy<string> parameterSetName, Action<dynamic, Dictionary<string, object>> valueFromPipeline)
        {
            this.owner = owner;
            Cmdlet = cmdlet;
            ((PSCmdlet)cmdlet).CommandRuntime = new DummyRuntime(owner);
            this.valueFromPipeline = valueFromPipeline;
            this.parameterSetName = parameterSetName;
        }

        public void BindParameters(Dictionary<string, object> boundParameters)
        {
            var properties = ReflectionCacheManager.Get(Cmdlet.GetType()).Properties.Where(p => p.GetAttribute<ParameterAttribute>() != null);

            foreach (var property in properties)
            {
                object propertyValue;

                var description = property.GetAttribute<DescriptionAttribute>();

                if ((description != null && boundParameters.TryGetValue(description.Description, out propertyValue)) || boundParameters.TryGetValue(property.Property.Name, out propertyValue))
                {
                    property.SetValue(Cmdlet, propertyValue);
                }
            }
        }

        public void BeginProcessing()
        {
            var method = Cmdlet.GetInternalMethod("SetParameterSetName");
            method.Invoke(Cmdlet, new[] { parameterSetName.Value });

            var myInvocation = Cmdlet.GetType().PSGetInternalFieldInfoFromBase("_myInvocation", "myInvocation");
            myInvocation.SetValue(Cmdlet, owner.MyInvocation);

            Cmdlet.BeginProcessingInternal();
        }

        public void ProcessRecord(Dictionary<string, object> boundParameters = null)
        {
            valueFromPipeline?.Invoke(Cmdlet, boundParameters);

            Cmdlet.ProcessRecordInternal();
        }

        public void EndProcessing()
        {
            Cmdlet.EndProcessingInternal();
        }

        public void StopProcessing()
        {
            Cmdlet.StopProcessingInternal();
        }
    }
}
