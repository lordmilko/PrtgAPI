using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.PowerShell
{
    public abstract class PrtgCmdlet : PSCmdlet
    {
        protected PrtgClient client => PrtgSessionState.Client;

        protected override void BeginProcessing()
        {
            if (PrtgSessionState.Client == null)
                throw new Exception("You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.");
        }

        protected void WriteList<T>(List<T> sendToPipeline)
        {
            var visibleMembers = typeof(T).GetPSVisibleMembers().ToList();

            visibleMembers.Sort();

            /*MoveToFront(visibleMembers, "Status");
            MoveToFront(visibleMembers, "Probe");
            MoveToFront(visibleMembers, "Group");
            MoveToFront(visibleMembers, "Device");
            MoveToFront(visibleMembers, "Name");*/

            foreach (var item in sendToPipeline)
            {
                var psObject = new PSObject(item);
                var propertySet = new PSPropertySet("DefaultDisplayPropertySet", visibleMembers);
                var memberSet = new PSMemberSet("PSStandardMembers", new[] { propertySet });
                psObject.Members.Add(memberSet);

                WriteObject(psObject);
            }
        }

        private void MoveToFront(List<string> list, string elm)
        {
            var item = list.Find(m => m == elm);
            list.Remove(item);
            list.Insert(0, item);
        }
    }
}
