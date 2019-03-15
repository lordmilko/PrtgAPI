namespace PrtgAPI.PowerShell.Base
{
    interface IPSCmdletEx
    {
        void BeginProcessingInternal();

        void ProcessRecordInternal();

        void EndProcessingInternal();

        void StopProcessingInternal();
    }
}