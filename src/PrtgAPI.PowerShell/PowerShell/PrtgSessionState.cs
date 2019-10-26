namespace PrtgAPI.PowerShell
{
    class PrtgSessionState
    {
        internal static PrtgClient Client { get; set; }
        internal static PSEdition? PSEdition { get; set; }
        internal static bool EnableProgress { get; set; }
    }
}
