using System;
using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using System.Linq;
using System.Threading.Tasks;
using PrtgAPI.Helpers;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// Retrieve sensors from a PRTG Server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Sensor")]
    public class GetSensor : PrtgTableCmdlet<Sensor, SensorParameters>
    {
        /// <summary>
        /// The device to retrieve sensors for.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// The probe to retrieve sensors for.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Probe Probe { get; set; }

        /// <summary>
        /// The group to retrieve sensors for.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public Group Group { get; set; }

        /// <summary>
        /// Only retrieve sensors that match a specific status.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public SensorStatus[] Status { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSensor"/> class.
        /// </summary>
        public GetSensor() : base(Content.Sensors, 500)
        {
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Device != null)
                AddPipelineFilter(Property.ParentId, Device.Id);
            if (Group != null)
                AddPipelineFilter(Property.Group, Group.Name);
            if (Probe != null)
                AddPipelineFilter(Property.Probe, Probe.Name);
            if (Status != null)
            {
                foreach (var value in Status)
                {
                    AddPipelineFilter(Property.Status, value);
                }
            }

            base.ProcessRecord();
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving sensors from a PRTG Server.
        /// </summary>
        /// <returns></returns>
        protected override SensorParameters CreateParameters() => new SensorParameters();
    }
}
