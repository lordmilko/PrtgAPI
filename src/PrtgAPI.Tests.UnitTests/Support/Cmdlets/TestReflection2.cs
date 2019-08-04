using System;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.Tests.UnitTests.PowerShell.Cmdlets
{
    [Cmdlet(VerbsDiagnostic.Test, "Reflection2")]
    public class TestReflection2 : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "ChainSourceId")]
        public SwitchParameter ChainSourceId { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Downstream")]
        public SwitchParameter Downstream { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "CmdletInput")]
        public SwitchParameter CmdletInput { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public object Object { get; set; }

        private PSReflectionCacheManager cacheManager;

        private long sourceId = -1;

        public TestReflection2()
        {
            cacheManager = new PSReflectionCacheManager(this);
        }

        protected override void ProcessRecord()
        {
            using (ProgressManager = new ProgressManager(this))
            {
                switch (ParameterSetName)
                {
                    case "ChainSourceId":
                        sourceId = ProgressManager.GetLastSourceId();
                        CommandRuntime.WriteProgress(sourceId, new ProgressRecord(2, $"Test-Reflection2 Activity for object '{Object}' with source ID '{sourceId}'", "Test-Reflection2 Description")
                        {
                            ParentActivityId = 1
                        });
                        Thread.Sleep(1);
                        WriteObject(Convert.ToInt32(((PSObject)Object).BaseObject) * 2);
                        break;

                    case "Downstream":
                        WriteObject(Object);
                        break;
                    case "CmdletInput":
                        WriteObject(cacheManager.GetCmdletPipelineInput().List, true);
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
                CommandRuntime.WriteProgress(sourceId, new ProgressRecord(2, "Activity", "Description")
                {
                    ParentActivityId = 1,
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
