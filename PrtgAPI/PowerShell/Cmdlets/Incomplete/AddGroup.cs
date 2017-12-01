using System;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
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
    /// <example>
    ///     <code>C:\> Get-Probe contoso | Add-Group Servers</code>
    ///     <para>Add a new group called "Servers" to the Contoso probe.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $params = New-GroupParameters Servers</code>
    ///     <para>C:\> $params.Tags = "awesomeGroup"</para>
    ///     <para>C:\> Get-Probe contoso | Add-Device $params</para>
    ///     <para>Add a new group called "Servers" with custom tags to the Contoso probe.</para>
    /// </example>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "Group", SupportsShouldProcess = true)]
    public class AddGroup : AddObject<NewGroupParameters, GroupOrProbe>
    {
        /// <summary>
        /// <para type="description">The name to use for the group.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Basic")]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Initializes a new instance of the <see cref="AddGroup"/> class.</para>
        /// </summary>
        public AddGroup() : base(BaseType.Group, CommandFunction.AddGroup2)
        {
        }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == "Basic")
            {
                Parameters = new NewGroupParameters(Name);
            }

            base.ProcessRecordEx();
        }
    }
}
