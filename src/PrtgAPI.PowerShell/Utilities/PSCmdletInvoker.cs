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
        public PSCmdlet CmdletToInvoke { get; }

        public List<object> Output => ((DummyRuntime)(CmdletToInvoke).CommandRuntime).Output;

        private Action<dynamic, Dictionary<string, object>> valueFromPipeline;

        private Lazy<string> parameterSetName;

        private PSCmdlet owner;

        private static Action<Cmdlet> beginProcessing;
        private static Action<Cmdlet> processRecord;
        private static Action<Cmdlet> endProcessing;
        private static Action<Cmdlet> stopProcessing;

        private static Func<object, object> memberwiseClone;

        static PSCmdletInvoker()
        {
            beginProcessing = ReflectionExtensions.CreateAction<Cmdlet>("BeginProcessing");
            processRecord = ReflectionExtensions.CreateAction<Cmdlet>("ProcessRecord");
            endProcessing = ReflectionExtensions.CreateAction<Cmdlet>("EndProcessing");
            stopProcessing = ReflectionExtensions.CreateAction<Cmdlet>("StopProcessing");

            memberwiseClone = ReflectionExtensions.CreateFunc<object, object>("MemberwiseClone");
        }

        public PSCmdletInvoker(PrtgCmdlet owner, PSCmdlet cmdlet, string parameterSetName, Action<dynamic, Dictionary<string, object>> valueFromPipeline = null) :
            this(owner, cmdlet, new Lazy<string>(() => parameterSetName), valueFromPipeline)
        {
        }

        public PSCmdletInvoker(PrtgCmdlet owner, PSCmdlet cmdlet, Lazy<string> parameterSetName, Action<dynamic, Dictionary<string, object>> valueFromPipeline)
        {
            this.owner = owner;
            CmdletToInvoke = cmdlet;
            cmdlet.CommandRuntime = new DummyRuntime(owner);
            this.valueFromPipeline = valueFromPipeline;
            this.parameterSetName = parameterSetName;
        }

        public void Invoke(Dictionary<string, object> boundParameters)
        {
            BindParameters(boundParameters);
            BeginProcessing(boundParameters);
            ProcessRecord(boundParameters);
            EndProcessing();
        }

        public void BindParameters(Dictionary<string, object> boundParameters)
        {
            var properties = ReflectionCacheManager.Get(CmdletToInvoke.GetType()).Properties.Where(p => p.GetAttribute<ParameterAttribute>() != null).ToArray();

            foreach (var property in properties)
            {
                object propertyValue;

                var description = property.GetAttribute<DescriptionAttribute>();

                if ((description != null && boundParameters.TryGetValue(description.Description, out propertyValue)) || boundParameters.TryGetValue(property.Property.Name, out propertyValue))
                {
                    property.SetValue(CmdletToInvoke, propertyValue);
                }
            }
        }

        public void BeginProcessing(Dictionary<string, object> boundParameters)
        {
            var method = CmdletToInvoke.GetInternalMethod("SetParameterSetName");
            method.Invoke(CmdletToInvoke, new[] { parameterSetName.Value });

            var myInvocationInfo = CmdletToInvoke.GetType().PSGetInternalFieldInfoFromBase("_myInvocation", "myInvocation");

            var newInvocation = (InvocationInfo) memberwiseClone(owner.MyInvocation);
            var boundParametersInfo = newInvocation.GetPublicPropertyInfo("BoundParameters");
            boundParametersInfo.SetValue(newInvocation, boundParameters);

            myInvocationInfo.SetValue(CmdletToInvoke, newInvocation);

            beginProcessing(CmdletToInvoke);
        }

        public void ProcessRecord(Dictionary<string, object> boundParameters = null)
        {
            valueFromPipeline?.Invoke(CmdletToInvoke, boundParameters);

            processRecord(CmdletToInvoke);
        }

        public void EndProcessing()
        {
            endProcessing(CmdletToInvoke);
        }

        public void StopProcessing()
        {
            stopProcessing(CmdletToInvoke);
        }
    }
}
