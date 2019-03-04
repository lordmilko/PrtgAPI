using System.Management.Automation;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves the current session's <see cref="PrtgClient"/></para>
    /// 
    /// <para type="description">The Get-PrtgClient cmdlet allows you to access the <see cref="PrtgClient"/> of the current session
    /// previously created with a call to Connect-PrtgServer. This allows you to view/edit the properties of the <see cref="PrtgClient"/>
    /// previously defined in your call to Connect-PrtgServer, as well as access the raw C# PrtgAPI should you wish to bypass or access
    /// methods not in the PowerShell interface.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-PrtgClient</code>
    ///     <para>View the settings of the session's PrtgClient. If the session does not have a PrtgClient, the cmdlet returns null</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> (Get-PrtgClient).RetryCount = 5</code>
    ///     <para>Change the RetryCount of the PrtgClient to 5</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> (Get-PrtgClient).GetSensors()</code>
    ///     <para>Invoke the GetSensors method of the PrtgClient</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Getting-Started#session-management-1">Online version:</para>
    /// <para type="link">Connect-PrtgServer</para>
    /// <para type="link">Disconnect-PrtgServer</para>
    /// <para type="link">Set-PrtgClient</para>
    /// </summary>
    [OutputType(typeof(PrtgClient))]
    [Cmdlet(VerbsCommon.Get, "PrtgClient")]
    public class GetPrtgClient : PSCmdlet
    {
        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            WriteObject(PrtgSessionState.Client);
        }
    }
}
