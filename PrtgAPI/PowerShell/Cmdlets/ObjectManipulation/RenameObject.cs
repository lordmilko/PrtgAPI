using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Rename a PRTG object.</para> //todo: talk about whatif and force. but we dont even have those?
    /// </summary>
    [Cmdlet(VerbsCommon.Rename, "Object", SupportsShouldProcess = true)]
    public class RenameObject : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The object to rename.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">The new name to give the object.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if(ShouldProcess($"'{Object.Name}' (ID: {Object.Id})"))
                client.RenameObject(Object.Id, Name);
        }
    }
}
