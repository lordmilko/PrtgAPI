using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Renames a PRTG object.</para>
    /// 
    /// <para type="description">The Rename-Object cmdlet allows you to rename a Sensor, Device, Group or Probe
    /// within PRTG. When renaming objects, it is recommended to first execute Rename-Object first with the -WhatIf
    /// parameter. This will show you all the objects that will be renamed when you execute the command normally.</para>
    /// 
    /// <para type="description">By default, Rename-Object will operate in Batch Mode. In Batch Mode, Rename-Object
    /// will not execute a request for each individual object, but will rather store each item in a queue to rename
    /// all objects at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects.</para>
    /// 
    /// <para type="description">If the pipeline is cancelled (either due to a cmdlet throwing an exception
    /// or the user pressing Ctrl-C) before fully completing, Rename-Object will not generate a request against PRTG.
    /// If you wish to disable Batch Mode and fully process objects individually one at a time, this can be achieved
    /// by specifying -Batch:$false.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor Memory | Rename-Object "Memory Free"</code>
    ///     <para>Rename all objects named "Memory" (case insensitive) to "Memory Free"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> Get-Sensor Memory | Rename-Object "Memory Free" -WhatIf
    ///         What if: Performing the operation "Rename-Object" on target "'Memory' (ID: 2001)"
    ///     </code>
    ///     <para>Preview what will happen when you attempt to rename all objects named "Memory"</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Organization#renaming-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Rename, "Object", SupportsShouldProcess = true)]
    public class RenameObject : PrtgMultiOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The object to rename.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// <para type="description">The new name to give the object.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        internal override string ProgressActivity => "Rename PRTG Objects";

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"'{Object.Name}' (ID: {Object.Id}) (New Name: {Name})"))
                ExecuteOrQueue(Object);                
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation()
        {
            ExecuteOperation(() => client.RenameObject(Object.Id, Name), $"Renaming {Object.GetTypeDescription().ToLower()} '{Object.Name}' to '{Name}'");
        }

        /// <summary>
        /// Invokes this cmdlet's action against all queued items from the pipeline.
        /// </summary>
        /// <param name="ids">The Object IDs of all queued items.</param>
        protected override void PerformMultiOperation(int[] ids)
        {
            ExecuteMultiOperation(() => client.SetObjectProperty(ids, ObjectProperty.Name, Name), $"Renaming {GetMultiTypeListSummary()} to '{Name}'");
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Object;
    }
}
