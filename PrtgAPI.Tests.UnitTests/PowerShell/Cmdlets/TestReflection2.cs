using System;
using System.Management.Automation;
using System.Threading;
using PrtgAPI.Helpers;
using PrtgAPI.PowerShell;

namespace PrtgAPI.Tests.UnitTests.PowerShell.Cmdlets
{
    [Cmdlet(VerbsDiagnostic.Test, "Reflection2")]
    public class TestReflection2 : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "ChainSourceId")]
        public SwitchParameter ChainSourceId { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Downstream")]
        public SwitchParameter Downstream { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "CmdletInput")]
        public SwitchParameter CmdletInput { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public object Object { get; set; }

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ChainSourceId":
                    var sourceId = ProgressManager.GetLastSourceId(CommandRuntime);
                    CommandRuntime.WriteProgress(sourceId, new ProgressRecord(2, $"Test-Reflection2 Activity for object '{Object}' with source ID '{sourceId}'", "Test-Reflection2 Description")
                    {
                        ParentActivityId = 1
                    });
                    Thread.Sleep(1000);
                    WriteObject(Convert.ToInt32(((PSObject)Object).BaseObject)*2);
                    break;

                case "Downstream":
                    WriteObject(Object);
                    break;
                case "CmdletInput":
                    WriteObject(CommandRuntime.GetCmdletPipelineInput(this).List, true);
                    break;

                default:
                    throw new NotImplementedException(ParameterSetName);
            }
        }
    }
}
