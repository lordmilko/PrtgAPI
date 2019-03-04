using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Enables PRTG Progress for the current PowerShell Session.</para>
    /// 
    /// <para type="description">The Enable-PrtgProgress enables displaying progress when piping between PrtgAPI cmdlets.
    /// By default, progress is enabled when PrtgAPI is used outside of scripts or the PowerShell ISE. Progress can then be manually
    /// controlled via the Enable-PrtgProgress and Disable-PrtgProgress cmdlets. If progress is manually enabled,
    /// this setting will only persist for the current PowerShell session, or until the next time Connect-PrtgServer is run.</para>
    /// 
    /// <example>
    ///     <code>C:\> Enable-PrtgProgress</code>
    ///     <para>Enable PrtgAPI progress for the current session</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Progress">Online version:</para>
    /// <para type="link">Disable-PrtgProgress</para>
    /// <para type="link">Connect-PrtgServer</para>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Enable, "PrtgProgress")]
    public class EnablePrtgProgress : PSCmdlet
    {
        /// <summary>
        /// Performs record-by-record processing for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            PrtgSessionState.EnableProgress = true;
        }
    }
}
