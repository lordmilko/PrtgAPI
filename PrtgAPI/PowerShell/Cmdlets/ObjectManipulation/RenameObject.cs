using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Renames a PRTG object.</para> //todo: talk about whatif and force. but we dont even have those?
    /// 
    /// <example>
    ///     <code>Get-Sensor Memory | Rename-Object "Memory Free"</code>
    ///     <para>Rename all objects named "Memory" (case insensitive) to "Memory Free"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>Get-Sensor Memory | Rename-Object "Memory Free" -WhatIf</code>
    ///     <para>What if: Performing the operation "Rename-Object" on target "'Memory' (ID: 2001)"</para>
    ///     <para>Preview what will happen when you attempt to rename all objects named "Memory"</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Rename, "Object", SupportsShouldProcess = true)]
    public class RenameObject : PrtgOperationCmdlet
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
            //todo when i piped a single entry to rename-object it didnt show progress

            //when i pipe a variable with a list in it for objectdata cmdlets im not getting progress

            if (ShouldProcess($"'{Object.Name}' (ID: {Object.Id})"))
            {
                ExecuteOperation(() =>
                {
                    if (Object.BaseType == BaseType.Probe)
                        client.SetObjectProperty(Object.Id, ObjectProperty.Name, Name);
                    else
                        client.RenameObject(Object.Id, Name);
                }, "Rename PRTG Object", $"Renaming {Object.BaseType.ToString().ToLower()} '{Object.Name}' to '{Name}'");
            }
                
        }
    }
}
