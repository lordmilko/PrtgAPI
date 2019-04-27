using System;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Cmdlets;

namespace PrtgAPI.PowerShell.Base
{
    internal static class InternalNotificationTriggerCommand
    {
        public static void ProcessRecordEx(PrtgOperationCmdlet cmdlet, Action<Action, string> executeOperation, TriggerParameters parameters, Action executeAndResolve = null, string whatIfAction = null, string whatIfTarget = null)
        {
            if (whatIfAction == null)
                whatIfAction = cmdlet.MyInvocation.MyCommand.Name;

            if (whatIfTarget == null)
                whatIfTarget = $"Object ID: {parameters.ObjectId} (Type: {parameters.Type}, Action: {parameters.OnNotificationAction})";

            if (cmdlet.ShouldProcess(whatIfTarget, whatIfAction))
            {
                if (cmdlet is AddNotificationTrigger)
                    executeOperation(() =>
                    {
                        if (executeAndResolve == null)
                            PrtgSessionState.Client.AddNotificationTrigger(parameters, false);
                        else
                            executeAndResolve();
                    }, $"Adding notification trigger '{parameters.OnNotificationAction?.Name ?? "None"}' to object ID {parameters.ObjectId}");
                else
                    executeOperation(() => PrtgSessionState.Client.SetNotificationTrigger(parameters), $"Updating notification trigger with ID {parameters.ObjectId} (Sub ID: {parameters.SubId})");
            }
        }
    }
}
