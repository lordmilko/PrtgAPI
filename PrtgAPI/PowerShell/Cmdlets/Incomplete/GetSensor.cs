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
    public class GetSensor : PrtgTableCmdlet<Sensor>
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

        private SensorStatus[] sensorstatus;

        /// <summary>
        /// Only retrieve sensors that match a specific status.
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public SensorStatus[] Status //we probably need to make a custom type that can take a sensorstatus or custom words we say like acknowledged, paused, etc
        {
            get { return sensorstatus; }
            set { sensorstatus = value; }
        }

        //todo - implement proper help system for use with get help or whatever? everything that can have it needs to provide help
        //todo: make the tags on an object an array instead of a string with the items separated by a space
        //do some research on how we can use the trace-command cmdlet for interesting purposes. could we use it to find out how exchange's resultsize type works?
        //we could also find the assembly its in and pull it apart to see how it works
        //if i make a request to prtg, cancel it, reopen powershell and make another request does it run fast or slow. this will tell us if its prtgs fault or ours it takes ages to load

        //
        //
        //try and find out how to "disable limits" for a channel type


        

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSensor"/> class.
        /// </summary>
        public GetSensor() : base(Content.Sensors, 500)
        {
        }

        private void ValidateFilters()
        {
            var statusFilter = Filter?.FirstOrDefault(f => f.Property == Property.Status);

            if (statusFilter != null)
            {
                if (statusFilter.Value.ToString().ToLower() == "paused")
                {
                    var list = Filter.ToList();
                    var index = list.IndexOf(statusFilter);
                    list.RemoveAt(index);

                    foreach(var status in new[] { SensorStatus.PausedByDependency, SensorStatus.PausedByLicense, SensorStatus.PausedBySchedule, SensorStatus.PausedByUser, SensorStatus.PausedUntil })
                    {
                        var filter = new SearchFilter(statusFilter.Property, statusFilter.Operator, status);
                        list.Insert(index, filter);
                    }

                    Filter = list.ToArray();
                }
                else
                {
                    SensorStatus temp;
                    var success = Enum.TryParse(statusFilter.Value.ToString(), true, out temp);

                    if (!success)
                        throw new Exception("i dont know what to do here");
                    else
                    {
                        var list = Filter.ToList();
                        var index = list.IndexOf(statusFilter);
                        list.RemoveAt(index);
                        statusFilter.Value = temp;
                        list.Insert(index, statusFilter);

                        Filter = list.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            //var b = client.GetSensorSettings(2197);
            //WriteObject(b);

            //var c = DateTime.ParseExact(b.MaintStart, "yyyy,MM,dd,HH,mm,ss", null);

            ValidateFilters();

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
            
            //

            base.ProcessRecord();
        }

        /// <summary>
        /// Retrieves all sensors from a PRTG Server.
        /// </summary>
        /// <returns>A list of all devices.</returns>
        protected override IEnumerable<Sensor> GetRecords()
        {
            //todo add support for getrawobject totals taking a params filter so we can get the totals when we have a filter. update prtgtablecmdlet accordingly

            return client.GetSensorsAsync();
        }

        /// <summary>
        /// Retrieves a list of sensors from a PRTG Server based on a specified filter.
        /// </summary>
        /// <param name="filter">A list of filters to use to limit search results.</param>
        /// <returns>A list of sensors that match the specified search criteria.</returns>
        protected override IEnumerable<Sensor> GetRecords(SearchFilter[] filter)
        {
            return client.GetSensors(filter);
        }
    }
}
