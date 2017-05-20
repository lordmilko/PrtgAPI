using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Helpers;

namespace PrtgAPI.Tests.UnitTests.PowerShell.Cmdlets
{
    [Cmdlet(VerbsDiagnostic.Test, "Reflection1")]
    public class TestReflection1 : PSCmdlet
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

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "SourceId":
                    WriteObject(CommandRuntime.GetLastProgressSourceId());
                    break;
                case "ChainSourceId":
                    WriteProgress(new ProgressRecord(1, "Test-Reflection1 Activity", "Test-Reflection1 Description"));
                    Thread.Sleep(1000);
                    WriteObject(new[] {1, 2, 3}, true);
                    break;
                case "Downstream":
                    WriteObject(CommandRuntime.GetDownstreamCmdlet().Name);
                    break;
                case "CmdletInput":
                    WriteObject(new[] {1,2,3}, true);
                    break;
                case "VariableInputArray":
                case "VariableInputObject":
                    WriteObject(CommandRuntime.GetPipelineInput(this), true);
                    break;

                default:
                    throw new NotImplementedException(ParameterSetName);
            }
        }
    }
}
