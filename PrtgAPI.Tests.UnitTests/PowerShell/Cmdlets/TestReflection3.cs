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
    [Cmdlet(VerbsDiagnostic.Test, "Reflection3")]
    public class TestReflection3 : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "ChainSourceId")]
        public SwitchParameter ChainSourceId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public object Object { get; set; }

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ChainSourceId":
                    var sourceId = CommandRuntime.GetLastProgressSourceId();
                    CommandRuntime.WriteProgress(sourceId, new ProgressRecord(3, $"Test-Reflection3 Activity for object '{Object}' with source ID '{sourceId}'", "Test-Reflection3 Description")
                    {
                        ParentActivityId = 2
                    });
                    Thread.Sleep(1000);
                    WriteObject(Convert.ToInt32(((PSObject)Object).BaseObject) * 2);
                    break;

                default:
                    throw new NotImplementedException(ParameterSetName);
            }
        }
    }
}
