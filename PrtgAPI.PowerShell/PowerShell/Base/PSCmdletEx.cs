using System.Management.Automation;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Provides enhanced <see cref="PSCmdlet"/> functionality, including members for invoking cmdlet processing methods internally.
    /// </summary>
    public class PSCmdletEx : PSCmdlet, IPSCmdletEx
    {
        void IPSCmdletEx.BeginProcessingInternal() => BeginProcessing();

        void IPSCmdletEx.ProcessRecordInternal() => ProcessRecord();

        void IPSCmdletEx.EndProcessingInternal() => EndProcessing();

        void IPSCmdletEx.StopProcessingInternal() => StopProcessing();
    }
}
