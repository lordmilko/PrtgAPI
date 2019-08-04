using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Disables PRTG Progress for the current PowerShell Session.</para>
    /// 
    /// <para type="description">The Disable-PrtgProgress disables displaying progress when piping between PrtgAPI cmdlets.
    /// By default, progress is enabled when PrtgAPI is used outside of scripts or the PowerShell ISE. Progress can then be manually
    /// controlled via the Enable-PrtgProgress and Disable-PrtgProgress cmdlets. If progress is manually disabled,
    /// this setting will only persist for the current PowerShell session, or until the next time Connect-PrtgServer is run.</para>
    /// 
    /// <example>
    ///     <code>C:\> Disable-PrtgProgress</code>
    ///     <para>Disable PrtgAPI progress for the current session</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Progress">Online version:</para>
    /// <para type="link">Enable-PrtgProgress</para>
    /// <para type="link">Connect-PrtgServer</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Disable, "PrtgProgress")]
    public class DisablePrtgProgress : PSCmdlet
    {
        /// <summary>
        /// Performs record-by-record processing for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            PrtgSessionState.EnableProgress = false;
        }
    }
}
