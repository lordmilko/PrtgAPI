using System.Management.Automation;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new set of group parameters for creating a brand new group under a group or probe.</para>
    /// 
    /// /// <para type="description">The New-GroupParameters cmdlet creates a set of parameters for adding a brand
    /// new group to PRTG. Group parameter objects returned from New-GroupParameters allow specifying
    /// a variety of group specific configuration details including auto-discovery settings at the time of object creation.</para>
    /// 
    /// <para type="description">Note that not all group parameters (such as settings that can be inherited from the parent group or probe)
    /// can be specified with PrtgAPI at the time of object creation. If you wish to modify such properties, this can be achieved
    /// after the group has been created via the Set-ObjectProperty cmdlet.</para>
    /// 
    /// <example>
    ///     <code>
    ///         C:\> $params = New-GroupParameters Servers
    ///
    ///         C:\> Get-Probe contoso | Add-Group $params
    ///     </code>
    ///     <para>Add a new group called "Servers" to the Contoso probe.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = New-GroupParameters Servers
    ///         C:\> $params.Tags = "awesomeGroup"
    ///
    ///         C:\> Get-Probe Contoso | Add-Group $params
    ///     </code>
    ///     <para>Add a new group called "Servers" to the Contoso probe with a custom tag "awesomeGroup".</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Creation#group-creation-1">Online version:</para>
    /// <para type="link">Add-Group</para>
    /// <para type="link">Set-ObjectProperty</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.New, "GroupParameters")]
    public class NewGroupParametersCommand : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name to give the new group.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            WriteObject(new NewGroupParameters(Name));
        }
    }
}
