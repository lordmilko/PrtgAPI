using System.Management.Automation;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Filters results on the PRTG Server to improve performance.</para>
    /// 
    /// <para type="description">The New-SearchFilter cmdlet allows you to filter the results returned by a cmdlet by a custom property.
    /// Each object returned from New-SearchFilter defines a single filter rule. Multiple filters can often be passed to compatible
    /// cmdlets to filter on multiple values. As a shorthand, you can access the New-SearchFilter cmdlet via the alias 'flt'.</para>
    /// 
    /// <para type="description">To view all properties that can be filtered on for on objects, see the SYNTAX or PARAMETERS section of this cmdlet's help page.</para> 
    /// 
    /// <example>
    ///     <code>C:\> flt parentid eq 1001 | Get-Sensor</code>
    ///     <para>Get all sensors under the object with ID 1001</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Filters#powershell">Online version:</para>
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
        /// <para type="description">Permits constructing search expressions believed to be incompatible
        /// with all versions of PRTG.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Illegal { get; set; }

        /// <summary>
        /// <para type="description">Specifies to avoid performing formatting the serialized filter value based on
        /// the required format of the specified filter property.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Raw { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Value is PSObject)
                Value = PSObjectUtilities.CleanPSObject(Value);

            FilterMode filterMode = FilterMode.Normal;

            if (Raw)
                filterMode = FilterMode.Raw;
            else
            {
                if (Illegal)
                    filterMode = FilterMode.Illegal;
            }

            WriteObject(new SearchFilter(Property, Operator, Value, filterMode));
        }
    }
}
