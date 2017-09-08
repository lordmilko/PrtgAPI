using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ObjectProperty")]
    public class GetObjectProperty : PrtgCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// The raw name of the property to retrieve. This can be typically discovered by inspecting the 'name' attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// Note: PRTG does not support retrieving raw section inheritance settings.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Raw")]
        public string RawProperty { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == "Raw")
            {
                WriteObject(client.GetObjectPropertyRaw(Object.Id, RawProperty));
            }
            else
            {
                switch (Object.BaseType)
                {
                    case BaseType.Sensor:
                        WriteObject(client.GetSensorProperties(Object.Id));
                        break;
                    case BaseType.Device:
                        WriteObject(client.GetDeviceProperties(Object.Id));
                        break;
                    case BaseType.Group:
                        WriteObject(client.GetGroupProperties(Object.Id));
                        break;
                    case BaseType.Probe:
                        WriteObject(client.GetProbeProperties(Object.Id));
                        break;
                }
            }
        }
    }
}
