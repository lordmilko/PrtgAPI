using System;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.Tests.UnitTests.PowerShell.Cmdlets
{
    [Cmdlet(VerbsDiagnostic.Test, "Reflection3")]
    public class TestReflection3 : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "ChainSourceId")]
        public SwitchParameter ChainSourceId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public object Object { get; set; }

        private long sourceId = -1;

        protected override void ProcessRecord()
        {
            using (ProgressManager = new ProgressManager(this))
            {
                switch (ParameterSetName)
                {
                    case "ChainSourceId":
                        sourceId = ProgressManager.GetLastSourceId();
                        CommandRuntime.WriteProgress(sourceId, new ProgressRecord(3, $"Test-Reflection3 Activity for object '{Object}' with source ID '{sourceId}'", "Test-Reflection3 Description")
                        {
                            ParentActivityId = 2
                        });
                        Thread.Sleep(1);
                        WriteObject(sourceId);
                        break;

                    default:
                        throw new NotImplementedException(ParameterSetName);
                }
            }
        }

        protected override void EndProcessing()
        {
            if (sourceId >= 0)
            {
                CommandRuntime.WriteProgress(sourceId, new ProgressRecord(3, "Activity", "Description")
                {
                    ParentActivityId = 2,
                    RecordType = ProgressRecordType.Completed
                });
            }
        }

        protected override void ProcessRecordEx()
        {
            throw new NotImplementedException();
        }
    }
}
