using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves probes from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-Probe cmdlet retrieves probes from a PRTG Server. Probes represent sites where the 
    /// PRTG Probe Service has been installed. Each PRTG Server has a single Core Probe. Additional probes may be installed
    /// with the PRTG Remote Probe installer for monitoring of remote sites.</para>
    /// 
    /// <para type="description">Get-Probe provides a variety of methods of filtering the probes requested from PRTG,
    /// including by probe name, probe status (Connected / Disconnected), ID and tags.
    /// Multiple filters can be used in conjunction to further limit the number of results returned. Probe properties that do
    /// not contain explicitly defined parameters on Get-Group can be specified as dynamic parameters, allowing one or more
    /// values to be specified of the specified type. All string parameters support the use of wildcards.</para>
    /// 
    /// <para type="description">For scenarios in which you wish to exert finer grained control over search filters,
    /// a custom <see cref="SearchFilter"/> object can be created by specifying the field name, condition and value
    /// to filter upon. For information on properties that can be filtered upon, see New-SearchFilter</para>
    /// 
    /// <para type="description">Get-Probe provides two parameter sets for filtering objects by tags. When filtering for probes
    /// that contain one of several tags, the -Tag parameter can be used, performing a logical OR between all specified operands.
    /// For scenarios in which you wish to filter for probes containing ALL specified tags, the -Tags
    /// parameter can be used, performing a logical AND between all specified operands.</para>
    /// 
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
    ///     <code>C:\> Get-Probe -TotalGroups 10</code>
    ///     <para>Get all probes that contain exactly 10 child groups using dynamic parameters.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> flt totalgroups eq 10 | Get-Probe</code>
    ///     <para>Get all probes that contain exactly 10 child groups using a SearchFilter.</para>
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
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Probes#powershell">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">New-SearchFilter</para>
    /// <para type="link">Approve-Probe</para>
    ///
    /// </summary>
    [OutputType(typeof(Probe))]
    [Cmdlet(VerbsCommon.Get, "Probe", DefaultParameterSetName = ParameterSet.LogicalAndTags)]
    public class GetProbe : PrtgTableStatusCmdlet<Probe, ProbeParameters>, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">Only retrieve probes that match a specified connection state.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public ProbeStatus[] ProbeStatus { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetProbe"/> class.
        /// </summary>
        public GetProbe() : base(Content.Probes, false)
        {
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            if (ProbeStatus != null)
                AddPipelineFilter(Property.ProbeStatus, ProbeStatus);

            base.ProcessAdditionalParameters();
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving probes from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override ProbeParameters CreateParameters() => new ProbeParameters();
    }
}
