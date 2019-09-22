using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Approves or denies a new probe for use within PRTG.</para>
    /// 
    /// <para type="description">The Approve-Probe cmdlet approves or denies a newly installed probe for use
    /// within PRTG. The Approve-Probe cmdlet can only be used on probes that have not yet been approved for use within
    /// PRTG. If Approve-Probe is executed on a probe that has already been approved, a warning will be emitted and the probe
    /// will be ignored.</para>
    /// 
    /// <para type="description">When a probe is approved, PRTG will automatically create the "Probe Device" object under the
    /// probe containing the default probe sensors. If the -<see cref="AutoDiscover"/> parameter is specified,
    /// PRTG will additionally attempt to perform an auto-discovery to add any devices present in the probe's network.</para>
    /// 
    /// <para type="description">If -<see cref="Deny"/> is specified, the probe will be removed from PRTG and its GID will be blacklisted.
    /// If a probe GID is blacklisted, it will not be able to communicate with PRTG unless its GID is changed or the GID is removed
    /// from the blacklist under Setup -> System Administration -> Core &amp; Probes</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Probe | Approve-Probe</code>
    ///     <para>Approve all PRTG probes. Any probes that have already been approved will be skipped.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe | where { ($_ | Get-ObjectProperty).ProbeApproved -eq $false } | Approve-Probe</code>
    ///     <para>Approve all probes that have not been approved.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Approve-Probe -Id 1001 -Deny</code>
    ///     <para>Deny the probe with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe | Approve-Probe -AutoDiscover</code>
    ///     <para>Approve all probes and immediately perform an auto-discovery. Any probes that have already been approved will be skipped.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Administrative-Tools#approvedeny-probes-1">Online version:</para>
    /// <para type="link">Get-Probe</para> 
    /// </summary>
    [Cmdlet(VerbsLifecycle.Approve, "Probe", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class ApproveProbe : PrtgPassThruCmdlet
    {
        /// <summary>
        /// <para type="description">Probe to set the approval status of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Deny)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.AutoDiscover)]
        public Probe Probe { get; set; }

        /// <summary>
        /// <para type="description">ID of the probe to set the approval status of.</para>
        /// </summary>
        [Alias("ProbeId")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.DenyManual)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.AutoDiscoverManual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">Deny the probe from communicating with PRTG. This will also blacklist the probe's GID.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Deny)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.DenyManual)]
        public SwitchParameter Deny { get; set; }

        /// <summary>
        /// <para type="description">Approve the probe and automatically perform an auto-discovery.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.AutoDiscover)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.AutoDiscoverManual)]
        public SwitchParameter AutoDiscover { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether to return the original <see cref="IObject"/> that was passed to this cmdlet, allowing the object to be further piped into additional cmdlets.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = ParameterSet.Deny)]
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = ParameterSet.AutoDiscover)]
        public new SwitchParameter PassThru
        {
            get { return base.PassThru; }
            set { base.PassThru = value; }
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var action = GetApproveAction();

            if (ShouldProcess(Probe != null ? Probe.Name : $"probe ID '{Id}'"))
            {
                ExecuteOperation(() =>
                {
                    var approved = client.GetProbeApprovalStatus(Probe?.Id ?? Id);

                    if (approved)
                        WriteWarning($"Skipping probe {GetProbeDescription()} as it is already approved.");
                    else
                        client.ApproveProbeInternal(Probe?.Id ?? Id, action);
                }, $"{GetActionDescription(action)}");
            }
        }

        private string GetActionDescription(ProbeApproval action)
        {
            string str = string.Empty;

            switch(action)
            {
                case ProbeApproval.Allow:
                    str = "Approving {0}";
                    break;
                case ProbeApproval.AllowAndDiscover:
                    str = "Approving {0} with auto-discovery";
                    break;
                case ProbeApproval.Deny:
                    str = "Denying {0}";
                    break;
            }

            if (Probe != null)
                return string.Format(str, $"probe '{Probe.Name}'");

            return string.Format(str, $"probe ID '{Id}'");
        }

        private string GetProbeDescription()
        {
            if (Probe != null)
                return $"'{Probe.Name}'";

            return $"ID '{Id}'";
        }

        private ProbeApproval GetApproveAction()
        {
            if (Deny)
                return ProbeApproval.Deny;

            if (AutoDiscover)
                return ProbeApproval.AllowAndDiscover;

            return ProbeApproval.Allow;
        }

        internal override string ProgressActivity => "Approving PRTG Probes";

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Probe;
    }
}
