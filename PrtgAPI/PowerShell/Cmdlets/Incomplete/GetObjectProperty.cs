using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ObjectProperty")]
    public class GetObjectProperty : PrtgCmdlet
    {
        //todo: have two parameter sets; either specify an object, or an id and a type

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public BaseType ObjectType { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            switch (ObjectType)
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

            //var settings = client.GetObjectProperties(Object.Id);
            //WriteObject(settings);
        }
    }
}
