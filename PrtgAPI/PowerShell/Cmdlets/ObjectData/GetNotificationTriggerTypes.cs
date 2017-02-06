using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve all notification trigger types supported by a PRTG Object.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "NotificationTriggerTypes")]
    public class GetNotificationTriggerTypes : PrtgCmdlet
    {
        /// <summary>
        /// The ID of the object to retrieve notification trigger types for.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public int Id { get; set; }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessPrtgRecord()
        {
            var types = client.GetNotificationTriggerTypes(Id);

            var obj = new PSObject();
            obj.Properties.Add(new PSNoteProperty("ObjectId", Id));
            obj.Properties.Add(new PSNoteProperty("Types", types));

            WriteObject(obj);
        }
    }
}
