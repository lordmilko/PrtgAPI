namespace PrtgAPI
{
    /// <summary>
    /// Tracks the progress of a PRTG Probe being restarted.
    /// </summary>
    public class ProbeRestartProgress
    {
        private Probe probe { get; set; }

        /// <summary>
        /// The ID of the probe that is being restarted.
        /// </summary>
        public int Id => probe.Id;

        /// <summary>
        /// The name of the probe that is being restarted.
        /// </summary>
        public string Name => probe.Name;

        /// <summary>
        /// The connection status of this probe before an attempt was made to restart it.
        /// </summary>
        public ProbeStatus InitialStatus => probe.ProbeStatus;

        /// <summary>
        /// Whether the probe has disconnected from PRTG as part of its restart.
        /// </summary>
        public bool Disconnected { get; set; }

        /// <summary>
        /// Whether the probe has reconnected to PRTG as part of its restart.
        /// </summary>
        public bool Reconnected { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeRestartProgress"/> class.
        /// </summary>
        /// <param name="probe">The probe that is being restarted.</param>
        public ProbeRestartProgress(Probe probe)
        {
            this.probe = probe;
        }
    }
}
