using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Clones a group within PRTG.</para>
    /// 
    /// <para type="description">The Clone-Group cmdlet duplicates a PRTG Group, including all devices and sensors defined under it.</para>
    /// <para type="description">To clone a group, you must specify the Object ID of the group or probe the cloned group will sit under.
    /// If a Name is not specified, the name of the group being cloned will be used.</para>
    /// <para type="description">When a group has been cloned, by default Clone-Group will attempt to resolve the object into a PrtgAPI Group.
    /// Based on the speed of your PRTG Server, this can sometimes result in a delay of 5-10 seconds due to the delay with which
    /// PRTG clones the object. If Clone-Group cannot resolve the resultant object on the first attempt, PrtgAPI will make a further
    /// 10 retries, pausing for a successively greater duration between each try. After each failed attempt a warning will be displayed indicating
    /// the number of attempts remaining. Object resolution can be aborted at any time by pressing an escape sequence such as Ctrl+C.</para>
    /// <para type="description">If you do not wish to resolve the resultant object, you can specify -Resolve:$false, which will
    /// cause Clone-Group to output a clone summary, including the object ID and name of the new object. As PRTG pauses all cloned objects
    /// by default, it is generally recommended to resolve the new object so that you may unpause the object with Resume-Object.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Group -Id 1234 | Clone-Group 5678</code>
    ///     <para>Clone the group with ID 1234 to the group or probe with ID 5678</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Id 1234 | Clone-Group 5678 MyNewGroup</code>
    ///     <para>Clone the group with ID 1234 to the group or probe with ID 5678 renamed as "MyNewGroup"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Id 1234 | Clone-Group 5678 -Resolve:$false</code>
    ///     <para>Clone the group with ID 1234 into the group or probe with ID 5678 without resolving the resultant PrtgObject</para>
    /// </example>
    /// 
    /// <para type="link">Get-Group</para>
    /// <para type="link">Clone-Sensor</para>
    /// <para type="link">Clone-Device</para>
    /// <para type="link">Resume-Object</para>
    /// </summary>
    [OutputType(typeof(Group))]
    [Cmdlet(VerbsCommon.Copy, "Group", SupportsShouldProcess = true)]
    public class CloneGroup : CloneSensorOrGroup<Group>
    {
        /// <summary>
        /// <para type="description">The group to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Group Group { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ShouldProcess($"{Group.Name} (ID: {Group.Id}, Destination: {DestinationId})"))
            {
                ProcessRecordEx(Group.Id, Group.Name, id => client.GetGroups(Property.Id, id));
            }
        }
    }
}