using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adjusts the position of an object within its parent.</para>
    /// 
    /// <para type="description">The Set-ObjectPosition cmdlet adjusts the position of an object under its parent node. Set-ObjectPosition
    /// allows you to specify the absolute position amongst the list siblings you'd like your object to occupy. If a value lower than 1 or higher
    /// than the total number of objects under the parent object is specified, PRTG will automatically place this object first or last in the list.</para>
    /// 
    /// <example>
    ///     <code>Get-Sensor -Id 3045 | Set-ObjectPosition 1</code>
    ///     <para>Move the sensor with ID 3045 to be first in the list under its parent.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ObjectPosition", SupportsShouldProcess = true)]
    public class SetObjectPosition : PrtgPassThruCmdlet
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
            if (ShouldProcess($"'{Object.Name}' (ID: {Object.Id}) (Current Position: {Object.Position}) (New Position: {Position})"))
                ExecuteOperation(() => client.SetPosition(Object, Position), $"Moving object {Object.Name} (ID: {Object.Id}) to position '{Position}'");
        }

        internal override string ProgressActivity => "Modify PRTG Object Positions";
    }
}
