namespace PrtgAPI.PowerShell.Base
{
    interface IWatchableCmdlet
    {
        int Interval { get; set; }

        bool WatchStream { get; set; }
    }
}
