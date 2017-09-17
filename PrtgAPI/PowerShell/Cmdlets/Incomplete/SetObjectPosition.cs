using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adjusts the position of an object within its parent.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ObjectPosition")]
    public class SetObjectPosition : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The object to move.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">Position to move the object to. If this value is higher than the number of items in the parent object,
        /// this object will be moved to the position closest possible position.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public int Position { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            client.SetPosition(Object, Position);
        }
    }
}
