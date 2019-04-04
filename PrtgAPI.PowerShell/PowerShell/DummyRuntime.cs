using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Management.Automation.Host;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell
{
    [ExcludeFromCodeCoverage]
    class DummyRuntime : ICommandRuntime
    {
        internal static long _lastUsedSourceId => (long) typeof(PSCmdlet).Assembly.GetType("System.Management.Automation.MshCommandRuntime").PSGetInternalStaticField("s_lastUsedSourceId", "_lastUsedSourceId");

        public List<object> Output { get; } = new List<object>();

        internal object PipelineProcessor => Owner.CommandRuntime.GetInternalProperty("PipelineProcessor");

        internal object InputPipe => Owner.CommandRuntime.GetInternalProperty("InputPipe");

        internal object OutputPipe => Owner.CommandRuntime.GetInternalProperty("OutputPipe");

        internal PrtgCmdlet Owner { get; }

        public DummyRuntime(PrtgCmdlet Owner)
        {
            this.Owner = Owner;
        }

        public void WriteObject(object sendToPipeline)
        {
            Output.Add(sendToPipeline);
        }

        public void WriteDebug(string text) => Owner.WriteDebug(text);

        public void WriteError(ErrorRecord errorRecord) => Owner.WriteError(errorRecord);

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (ObjectExtensions.IsIEnumerable(sendToPipeline))
            {
                foreach (var o in ObjectExtensions.ToIEnumerable(sendToPipeline))
                    Output.Add(o);
            }
            else
                Output.Add(sendToPipeline);
        }

        public void WriteProgress(ProgressRecord progressRecord) => Owner.WriteProgress(progressRecord);

        public void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
            Owner.CommandRuntime.WriteProgress(sourceId, progressRecord);
        }

        public void WriteVerbose(string text)
        {
            Owner.WriteVerbose(text);
        }

        public void WriteWarning(string text)
        {
            Owner.WriteWarning(text);
        }

        public void WriteCommandDetail(string text) => Owner.WriteCommandDetail(text);

        public bool ShouldProcess(string target) => Owner.ShouldProcess(target);

        public bool ShouldProcess(string target, string action) => Owner.ShouldProcess(target, action);

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption) =>
            Owner.ShouldProcess(verboseDescription, verboseWarning, caption);

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason) =>
            Owner.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);

        public bool ShouldContinue(string query, string caption) => Owner.ShouldContinue(query, caption);

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll) =>
            Owner.ShouldContinue(query, caption, ref yesToAll, ref noToAll);

        public bool TransactionAvailable() => Owner.TransactionAvailable();

        public void ThrowTerminatingError(ErrorRecord errorRecord) => Owner.ThrowTerminatingError(errorRecord);

        public PSHost Host => Owner.Host;

        public PSTransactionContext CurrentPSTransaction => Owner.CurrentPSTransaction;
    }
}
