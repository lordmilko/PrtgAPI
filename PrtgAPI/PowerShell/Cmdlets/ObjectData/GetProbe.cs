using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves probes from a PRTG Server.</para>
    /// </summary>
    /// 
    /// <para type="description">The Get-Probe cmdlet retrieves probes from a PRTG Server. Probes represent sites where the 
    /// PRTG Probe Service has been installed. Each PRTG Server has a single Core Probe. Additional probes may be installed
    /// with the PRTG Remote Probe installer for monitoring of remote sites. Get-Probe provides a variety of methods of filtering
    /// the probes requested from PRTG, including by probe name, ID and tags. Multiple filters can be used in conjunction to further
    /// limit the number of results returned.</para>
    /// <para type="description">For scenarios in which you wish to filter on properties not covered by the parameters available
    /// in Get-Probe, a custom <see cref="SearchFilter"/> object can be created by specifying the field name, condition and value
    /// to filter upon. For information on properties that can be filtered upon, see New-SearchFilter</para>
    /// <para type="description">The <see cref="Probe"/> objects returned from Get-Probe can be piped to a variety of other cmdlets for
    /// further processing, including Get-Group, Get-Device and Get-Sensor, where the ID or name of the probe will be used to filter
    /// for child objects of the specified type.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Probe contoso</code>
    ///     <para>Get all probes named "contoso" (case insensitive)</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe *office*</code>
    ///     <para>Get all probes whose name contains "office" (case insensitive)</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe -Id 1</code>
    ///     <para>Get the probe with ID 1</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe -Count 1</code>
    ///     <para>Get only 1 probe from PRTG.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> flt totalsens greaterthan 100 | Get-Probe</code>
    ///     <para>Get all probes that contain more than 100 sensors.</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">New-SearchFilter</para>
    [OutputType(typeof(Probe))]
    [Cmdlet(VerbsCommon.Get, "Probe")]
    public class GetProbe : PrtgTableStatusCmdlet<Probe, ProbeParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetProbe"/> class.
        /// </summary>
        public GetProbe() : base(Content.ProbeNode, null)
        {
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving probes from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override ProbeParameters CreateParameters() => new ProbeParameters();
    }
}
