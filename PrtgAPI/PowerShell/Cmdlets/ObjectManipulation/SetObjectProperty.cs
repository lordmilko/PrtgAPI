using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modify the value of an object property.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ObjectProperty", SupportsShouldProcess = true)]
    public class SetObjectProperty : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The object to modify the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// <para type="description">The property to modify.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public ObjectProperty Property { get; set; }

        /// <summary>
        /// <para type="description">The value to set for the specified property.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public object Value { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if(ShouldProcess($"{Object.Name} (ID: {Object.Id})", $"Set-ObjectProperty {Property} = '{Value}'"))
                client.SetObjectProperty(Object.Id, Property, Value);
        }
    }
}
