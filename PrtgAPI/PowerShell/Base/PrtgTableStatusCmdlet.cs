using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables that support enhanced record filters and are capable of filtering by <see cref="Status"/> (sensors, devices, probes, etc).
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableStatusCmdlet<TObject, TParam> : PrtgTableTagCmdlet<TObject, TParam>
        where TParam : TableParameters<TObject>
        where TObject : SensorOrDeviceOrGroupOrProbe
    {
        private bool statusProcessed;

        /// <summary>
        /// <para type="description">Only retrieve objects that match a specific status.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public Status[] Status { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableStatusCmdlet{TObject,TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="shouldStream">Whether this cmdlet should have streaming enabled.</param>
        public PrtgTableStatusCmdlet(Content content, bool? shouldStream) : base(content, shouldStream)
        {
        }

        internal void ProcessStatusFilter()
        {
            if (Status != null)
            {
                statusProcessed = true;

                foreach (var value in Status)
                {
                    AddPipelineFilter(Property.Status, value);
                }
            }
        }

        /// <summary>
        /// Process any post retrieval filters specific to the current cmdlet.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <returns>The filtered records.</returns>
        protected override IEnumerable<TObject> PostProcessAdditionalFilters(IEnumerable<TObject> records)
        {
            records = FilterResponseRecordsByStatus(records);

            return base.PostProcessAdditionalFilters(records);
        }

        /// <summary>
        /// Specifies how the records returned from this cmdlet should be sorted.
        /// </summary>
        /// <param name="records">The records to sort.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> representing the sorted collection.</returns>
        protected override IEnumerable<TObject> SortReturnedRecords(IEnumerable<TObject> records)
        {
            return records.OrderBy(r => r.Position);
        }

        private IEnumerable<TObject> FilterResponseRecordsByStatus(IEnumerable<TObject> records)
        {
            //Devices, Groups and Probes do not always filter by Status properly (if at all). To correct
            //for this, we abstain from filtering server side and instead filter the PRTG Response
            if (!statusProcessed && Status != null)
            {
                List<Status> flags = new List<Status>();

                foreach (var e in Status)
                {
                    if (e == 0)
                        flags.Add(e);
                    else
                    {
                        var underlying = e.GetUnderlyingFlags().Cast<Status>().ToList();

                        if (underlying.Count > 0)
                            flags.AddRange(underlying);
                        else
                            flags.Add(e);
                    }
                }

                records = records.Where(r => flags.Any(f => f == r.Status));
            }

            return records;
        }
    }
}
