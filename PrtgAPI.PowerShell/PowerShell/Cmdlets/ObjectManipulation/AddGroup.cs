using System;
using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adds a new group to a PRTG Group or Probe.</para>
    /// 
    /// <para type="description">The Add-Group cmdlet adds a new group to a a PRTG Group or Probe. When adding a new
    /// group, Add-Group supports two methods of specifying the parameters required to create the object. For basic scenarios
    /// where you inherit all settings from the parent object, a group can created by passing nothing more than a <see cref="Name"/>
    /// to Add-Group.</para>
    /// <para type="description">For more advanced scenarios where you wish to specify more advanced parameters (such as Tags that will apply to the group)
    /// a <see cref="NewGroupParameters"/> object can be instead created with the New-GroupParameters cmdlet.
    /// When the parameters object is passed to Add-Device, PrtgAPI will validate that all mandatory parameter fields contain values.
    /// If a mandatory field is missing a value, Add-Sensor will throw an <see cref="InvalidOperationException"/>, listing the field whose value was missing.</para>
    /// 
    /// <para type="description">By default, Add-Group will attempt to resolve the created group to a
    /// <see cref="Group"/> object. As PRTG does not return the ID of the created object, PrtgAPI
    /// identifies the newly created group by comparing the groups under the parent object before and after the new group is created.
    /// While this is generally very reliable, in the event something or someone else creates another new group directly
    /// under the target object with the same Name, that object will also be returned in the objects
    /// resolved by Add-Group. If you do not wish to resolve the created group, this behavior can be
    /// disabled by specifying -Resolve:$false.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Probe contoso | Add-Group Servers</code>
    ///     <para>Add a new group called "Servers" to the Contoso probe.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = New-GroupParameters Servers
    ///         C:\> $params.Tags = "awesomeGroup"
    ///
    ///         C:\> Get-Probe contoso | Add-Device $params
    ///     </code>
    ///     <para>Add a new group called "Servers" with custom tags to the Contoso probe.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Creation#groups-1">Online version:</para>
    /// <para type="link">New-GroupParameters</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "Group", SupportsShouldProcess = true)]
    public class AddGroup : AddParametersObject<NewGroupParameters, Group, GroupOrProbe>
    {
        /// <summary>
        /// <para type="description">The parent object to create an object under.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Basic)]
        public new GroupOrProbe Destination
        {
            get { return base.Destination; }
            set { base.Destination = value; }
        }

        /// <summary>
        /// <para type="description">The name to use for the group.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Basic)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Initializes a new instance of the <see cref="AddGroup"/> class.</para>
        /// </summary>
        public AddGroup() : base(BaseType.Group)
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == ParameterSet.Basic)
            {
                Parameters = new NewGroupParameters(Name);
            }

            base.ProcessRecordEx();
        }

        /// <summary>
        /// Resolves the children of the destination object that match the new object's name.
        /// </summary>
        /// <param name="filters">An array of search filters used to retrieve all children of the destination with the specified name.</param>
        /// <returns>All objects under the parent object that match the new object's name.</returns>
        protected override List<Group> GetObjects(SearchFilter[] filters) => client.GetGroups(filters);
    }
}
