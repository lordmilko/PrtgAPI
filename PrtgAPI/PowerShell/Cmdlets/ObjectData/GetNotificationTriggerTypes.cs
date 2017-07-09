using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves all notification trigger types supported by a PRTG Object.</para>
    /// 
    /// <para type="description">The Get-NotificationTriggerTypes cmdlet retrieves a list of all notification trigger
    /// types supported by a PRTG Object. Certain objects (such as sensors) only support certain types of notification
    /// triggers depending on what the object is used for (e.g. network traffic sensors vs disk uage sensors).
    /// Attempting to add a trigger of an unsupported type with Add-NotificationTrigger will generate an
    /// <see cref="InvalidTriggerTypeException"/>.</para>
    /// 
    /// <example>
    ///     <code>Get-Sensor -Id 2001 | Get-NotificationTriggerTypes</code>
    ///     <para>Get all notification trigger types supported by the sensor with object ID 2001.</para>
    /// </example>
    /// 
    /// <para type="link">Get-NotificationTrigger</para>
    /// <para type="link">New-NotificationTriggerParameter</para>
    /// <para type="link">Add-NotificationTrigger</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "NotificationTriggerTypes")]
    public class GetNotificationTriggerTypes : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The ID of the object to retrieve notification trigger types for.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public int Id { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var types = client.GetNotificationTriggerTypes(Id);

            var obj = new PSObject();
            obj.Properties.Add(new PSNoteProperty("ObjectId", Id));
            obj.Properties.Add(new PSNoteProperty("Types", types));

            WriteObject(obj);
        }
    }
}
