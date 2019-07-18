using System;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.Tests.UnitTests.PowerShell.Cmdlets
{
    [Cmdlet(VerbsDiagnostic.Test, "Reflection1")]
    public class TestReflection1 : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "SourceId")]
        public SwitchParameter SourceId { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "ChainSourceId")]
        public SwitchParameter ChainSourceId { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Downstream")]
        public SwitchParameter Downstream { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "CmdletInput")]
        public SwitchParameter CmdletInput { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "VariableInputArray")]
        public SwitchParameter VariableInputArray { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "VariableInputObject")]
        public SwitchParameter VariableInputObject { get; set; }

        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public object Object { get; set; }

        private PSReflectionCacheManager cacheManager;

        ProgressRecord record;

        public TestReflection1()
        {
            cacheManager = new PSReflectionCacheManager(this);
        }

        protected override void ProcessRecord()
        {
            using (ProgressManager = new ProgressManager(this))
            {
                switch (ParameterSetName)
                {
                    case "SourceId":
                        WriteObject(ProgressManager.GetLastSourceId());
                        break;
                    case "ChainSourceId":
                        record = new ProgressRecord(1, "Test-Reflection1 Activity", "Test-Reflection1 Description");
                        WriteProgress(record);
                        Thread.Sleep(1);
                        WriteObject(new[] { 1, 2, 3 }, true);
                        break;
                    case "Downstream":
                        WriteObject(cacheManager.GetDownstreamCmdletInfo().Name);
                        break;
                    case "CmdletInput":
                        WriteObject(new[] { 1, 2, 3 }, true);
                        break;
                    case "VariableInputArray":
                    case "VariableInputObject":
                        WriteObject(cacheManager.GetCmdletPipelineInput().List, true);
                        break;

                    default:
                        throw new NotImplementedException(ParameterSetName);
                }
            }
        }

        protected override void EndProcessing()
        {
            if (record != null)
            {
                record.RecordType = ProgressRecordType.Completed;

                WriteProgress(record);
            }
        }

        protected override void ProcessRecordEx()
        {
            throw new NotImplementedException();
        }
    }
}
