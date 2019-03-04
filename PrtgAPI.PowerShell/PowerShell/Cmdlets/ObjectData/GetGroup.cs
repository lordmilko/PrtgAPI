using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Linq.Expressions;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves groups from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-Group cmdlet retrieves groups from a PRTG Server. Groups allow you to organize devices and other groups together.
    /// In addition, the root node of PRTG containing all probes in the system is marked as a group. Get-Group provides a variety of methods of
    /// filtering the groups requested from PRTG, including by group name, ID and tags, as well as by parent probe/group. Multiple filters can be
    /// used in conjunction to further limit the number of results returned. Group properties that do not contain explicitly defined
    /// parameters on Get-Group can be specified as dynamic parameters, allowing one or more values to be specified of the specified type. All string parameters
    /// support the use of wildcards.</para>
    /// 
    /// <para type="description">For scenarios in which you wish to exert finer grained control over search filters,
    /// a custom <see cref="SearchFilter"/> object can be created by specifying the field name, condition and value to filter upon.
    /// For more information on properties that can be filtered upon, see New-SearchFilter.</para>
    /// 
    /// <para type="description">Get-Group provides two parameter sets for filtering objects by tags. When filtering for groups
    /// that contain one of several tags, the -Tag parameter can be used, performing a logical OR between all specified operands.
    /// For scenarios in which you wish to filter for groups containing ALL specified tags, the -Tags
    /// parameter can be used, performing a logical AND between all specified operands.</para>
    /// 
    /// <para type="description">When requesting groups belonging to a specified group, PRTG will not return any objects that may
    /// be present under further child groups of the parent group (i.e. grandchildren). To work around this, by default Get-Groups will automatically recurse
    /// child groups if it detects the initial group request did not return all items (as evidenced by the group's TotalGroups property.
    /// If you do not wish Get-Group to recurse child groups, this behavior can be overridden by specifying -Recurse:$false.</para>
    /// 
    /// <para type="description">The <see cref="Group"/> objects returned from Get-Group can be piped to a variety of other cmdlets for further processing,
    /// including Get-Group, Get-Device and Get-Sensor, where the ID or name of the group will be used to filter for child objects of the specified type.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Group Servers</code>
    ///     <para>Get all groups named "Servers" (case insensitive)</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group *windows*</code>
    ///     <para>Get all groups whose name contains "windows" (case insensitive)</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Id 2005</code>
    ///     <para>Get the group with ID 2005</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe contoso | Get-Group</code>
    ///     <para>Get all groups under the probe named "contoso"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Tag ny,ca</code>
    ///     <para>Get all devices that have the tag "ny" or "ca"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Tags ny,south</code>
    ///     <para>Get all groups in South New York</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -ParentId 3045</code>
    ///     <para>Get all groups directly under the object with ID 3045 using a dynamic parameter.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> flt parentid eq 3045 | Get-Group</code>
    ///     <para>Get all groups directly under the object with ID 3045 using a SearchFilter.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Count 1</code>
    ///     <para>Get only 1 group from PRTG.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Id 2001 | Get-Group -Recurse:$false</code>
    ///     <para>Get all groups directly under the group with ID 2001, ignoring all grandchildren.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Groups#powershell">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">New-SearchFilter</para>
    /// <para type="link">Add-Group</para> 
    /// </summary>
    [OutputType(typeof(Group))]
    [Cmdlet(VerbsCommon.Get, "Group", DefaultParameterSetName = LogicalAndTags)]
    public class GetGroup : PrtgTableRecurseCmdlet<Group, GroupParameters>, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">The parent group to retrieve groups for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Group Group { get; set; }

        /// <summary>
        /// <para type="description">The probe to retrieve groups for.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public NameOrObject<Probe> Probe { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetGroup"/> class.
        /// </summary>
        public GetGroup() : base(Content.Groups, false)
        {
        }

        internal override bool StreamCount()
        {
            return !(Group != null && Recurse);
        }

        internal override List<Group> GetObjectsInternal(GroupParameters parameters)
        {
            //No point getting child groups now, since we'll get them anyway when we get additional records
            if (Group != null && Recurse && Group.TotalGroups > 0)
                return new List<Group>();

            return base.GetObjectsInternal(parameters);
        }

        /// <summary>
        /// Retrieves additional records not included in the initial request.
        /// </summary>
        /// <param name="parameters">The parameters that were used to perform the initial request.</param>
        protected override IEnumerable<Group> GetAdditionalRecords(GroupParameters parameters)
        {
            //If recursing, get all groups under the parent group without filtering by any
            //specified filters such as Name, etc
            var records = GetAdditionalGroupRecords(Group, g => g.TotalGroups, parameters);

            //Then, if any search filters were specified, translate the specified filters into an Expression
            //used to generate a lambda function capable of filtering the returned records side
            if (parameters.SearchFilters != null)
            {
                var validFilters = parameters.SearchFilters.Where(f => f.Property != Property.ParentId).ToArray();

                var filter = SearchFilterToExpression.Parse<Group>(validFilters);

                return records.Where(filter);
            }

            return records;
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            if (Probe != null)
                AddNameOrObjectFilter(Property.Probe, Probe, p => p.Name);
            else if (Group != null)
                AddPipelineFilter(Property.ParentId, Group.Id);

            base.ProcessAdditionalParameters();
        }

        /// <summary>
        /// Process any post retrieval filters specific to the current cmdlet.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <returns>The filtered records.</returns>
        protected override IEnumerable<Group> PostProcessAdditionalFilters(IEnumerable<Group> records)
        {
            records = FilterResponseRecordsByNameOrObjectName(Probe, r => r.Probe, records);

            return base.PostProcessAdditionalFilters(records);
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving groups from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override GroupParameters CreateParameters() => new GroupParameters();
    }
}
