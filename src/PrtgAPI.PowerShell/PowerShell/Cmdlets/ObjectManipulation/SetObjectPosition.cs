using System;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adjusts the position of an object within its parent.</para>
    /// 
    /// <para type="description">The Set-ObjectPosition cmdlet adjusts the position of an object under its parent node. Set-ObjectPosition
    /// allows you to specify either the direction to move the object in (Up, Down, Top, Bottom) or an absolute number to move the object to a fixed position.
    /// If a numeric position lower than 1 or higher than the total number of objects under the parent object is specified, PRTG will automatically move the
    /// object to the top or bottom of the list.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 3045 | Set-ObjectPosition 1</code>
    ///     <para>Move the sensor with ID 3045 to be first in the list under its parent.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | Set-ObjectPosition Up</code>
    ///     <para>Move the sensor with ID 1001 up a single position from its current location.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Set-ObjectPosition -Id 1001 Up</code>
    ///     <para>Move the object with ID 1001 uo a single position from its current location.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Organization#positioning-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ObjectPosition", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class SetObjectPosition : PrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">The object to reposition.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">ID of the object to reposition.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">Position to move the object to. If this value is higher than the number of items in the parent object,
        /// this object will be moved to the position closest possible position.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public Position Position { get; set; }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (Object == null)
            {
                var obj = client.GetObject(Id, true);

                Object = obj as SensorOrDeviceOrGroupOrProbe;

                if (Object == null)
                    throw new InvalidOperationException($"Cannot modify position of object '{obj}' (ID: {obj.Id}, Type: {obj.Type}). Object must be a sensor, device, group or probe.");
            }

            if (ShouldProcess($"'{Object.Name}' (ID: {Object.Id}) (Current Position: {Object.Position}) (New Position: {Position})"))
            {
                ExecuteOperation(() =>
                {
                    if (Position.IsAbsolutePosition)
                        client.SetPosition(Object, Position.AbsolutePosition);
                    else
                        client.SetPosition(Object.Id, Position.RelativePosition);

                }, $"Moving object {Object.Name} (ID: {Object.Id}) to position '{Position}'");
            }
        }

        internal override string ProgressActivity => "Modify PRTG Object Positions";

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Object;
    }
}
