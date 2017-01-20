using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve devices from a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Device")]
    public class GetDevice : PrtgTableCmdlet<Device, DeviceParameters>
    {
        /// <summary>
        /// The group to retrieve devices for.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Group Group { get; set; }

        /// <summary>
        /// The probe to retrieve devices for.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Probe Probe { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetDevice"/> class.
        /// </summary>
        public GetDevice() : base(Content.Devices, null)
        {
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Probe != null)
                AddPipelineFilter(Property.Probe, Probe.Name);
            else if (Group != null)
                AddPipelineFilter(Property.ParentId, Group.Id);

            base.ProcessRecord();
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving devices from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        protected override DeviceParameters CreateParameters() => new DeviceParameters();
    }
}
