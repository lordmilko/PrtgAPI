using System.Management.Automation;
using PrtgAPI.Helpers;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Filters results on the PRTG Server to improve performance.</para>
    /// 
    /// <para type="description">The New-SearchFilter cmdlet allows you to filter the results returned by a cmdlet by a custom property.
    /// Each object returned from New-SearchFilter defines a single filter rule. Multiple filters can often be passed to compatible
    /// cmdlets to filter on multiple values.</para>
    /// <para type="description">To view all properties that can be filtered on for on objects, see the SYNTAX or PARAMETERS section of this cmdlet's help page.</para> 
    /// 
    /// <example>
    ///     <code>C:\> flt parentid eq 1001 | Get-Sensor</code>
    ///     <para>Get all sensors under the object with ID 1001</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [OutputType(typeof(SearchFilter))]
    [Cmdlet(VerbsCommon.New, "SearchFilter")]
    public class NewSearchFilter : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Object property to filter on.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public Property Property { get; set; }

        /// <summary>
        /// <para type="description">Operator to filter with.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1)]
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// <para type="description">Value to filter for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 2)]
        public object Value { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Value is PSObject)
                Value = PSObjectHelpers.CleanPSObject(Value);

            WriteObject(new SearchFilter(Property, Operator, Value));
        }
    }
}
