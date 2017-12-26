using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables that support enhanced record filters (sensors, devices, probes, etc)
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableCmdlet<TObject, TParam> : PrtgTableBaseCmdlet<TObject, TParam> where TParam : TableParameters<TObject> where TObject : SensorOrDeviceOrGroupOrProbe
    {
        /// <summary>
        /// <para type="description">Retrieve an object with a specified ID.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Retrieve an obejct with a specified ID.")]
        public int[] Id { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects with certain tags. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Filter the response to objects with certain tags. Can include wildcards.")]
        public string[] Tags { get; set; }

        /// <summary>
        /// <para type="description">Only retrieve objects that match a specific status.</para>
        /// </summary>
        [Parameter(ValueFromPipeline = true)]
        public Status[] Status { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableCmdlet{TObject,TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="progressThreshold">The numeric threshold at which this cmdlet should show a progress bar when retrieving results.</param>
        public PrtgTableCmdlet(Content content, int? progressThreshold) : base(content, progressThreshold)
        {
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            ProcessIdFilter();
            ProcessTagsFilter();
            ProcessStatusFilter();

            base.ProcessAdditionalParameters();
        }

        private void ProcessIdFilter()
        {
            if (Id != null)
            {
                foreach (var id in Id)
                {
                    AddPipelineFilter(Property.Id, id);
                }
            }
        }

        private void ProcessTagsFilter()
        {
            if (Tags != null)
            {
                foreach (var value in Tags)
                {
                    AddWildcardFilter(Property.Tags, value);
                }
            }
        }

        private void ProcessStatusFilter()
        {
            if (Status != null)
            {
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
            records = FilterResponseRecordsByTag(records);

            records = records.OrderBy(r => r.Position);

            return base.PostProcessAdditionalFilters(records);
        }

        private IEnumerable<TObject> FilterResponseRecordsByTag(IEnumerable<TObject> records)
        {
            if (Tags != null)
            {
                //Select all records where at least one of the filter tags is present
                records = from record in records
                          from tag in Tags
                          let filter = new WildcardPattern(tag, WildcardOptions.IgnoreCase)
                          from t in record.Tags
                          where filter.IsMatch(t)
                          select record;
            }

            return records;
        }
    }
}
