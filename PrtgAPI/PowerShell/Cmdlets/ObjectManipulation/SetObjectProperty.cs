using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Modify the value of an object property.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ObjectProperty", SupportsShouldProcess = true)]
    public class SetObjectProperty : PrtgCmdlet
    {
        /// <summary>
        /// The object to modify the properties of.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// The property to modify.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public ObjectProperty Property { get; set; }

        /// <summary>
        /// The value to set for the specified property.
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
