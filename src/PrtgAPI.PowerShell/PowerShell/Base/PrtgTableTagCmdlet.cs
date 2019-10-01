using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables that support enhanced record filters and are capable of filtering by tags (sensors, devices, probes, etc).
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableTagCmdlet<TObject, TParam> : PrtgTableFilterCmdlet<TObject, TParam>
        where TObject : ITableObject, IObject
        where TParam : TableParameters<TObject>
    {
        /// <summary>
        /// <para type="description">Filter the response to objects with all specified tags. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.LogicalAndTags, HelpMessage = "Filter the response to objects with all specified tags. Can include wildcards.")]
        public string[] Tags { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects with one of several tags. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.LogicalOrTags, HelpMessage = "Filter the response to objects with one of several tags. Can include wildcards.")]
        public string[] Tag { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableTagCmdlet{TObject,TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="shouldStream">Whether this cmdlet should have streaming enabled.</param>
        public PrtgTableTagCmdlet(Content content, bool? shouldStream) : base(content, shouldStream)
        {
        }

        private void ProcessLogicalAndTagsFilter()
        {
            if (Tags != null)
            {
                AddWildcardFilter(Property.Tags, string.Join(",", Tags.SelectMany(CleanWildcard)));
            }
        }

        private void ProcessLogicalOrTagsFilter()
        {
            ProcessWildcardArrayFilter(Property.Tags, Tag);
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            if (ParameterSetName == ParameterSet.LogicalAndTags)
                ProcessLogicalAndTagsFilter();
            else if (ParameterSetName == ParameterSet.LogicalOrTags)
                ProcessLogicalOrTagsFilter();
            else
                throw new NotImplementedException($"Don't know how to process tags for parameter set '{ParameterSetName}'.");

            base.ProcessAdditionalParameters();
        }

        /// <summary>
        /// Process any post retrieval filters specific to the current cmdlet.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <returns>The filtered records.</returns>
        protected override IEnumerable<TObject> PostProcessAdditionalFilters(IEnumerable<TObject> records)
        {
            if (ParameterSetName == ParameterSet.LogicalAndTags)
                records = FilterResponseRecordsByLogicalAndTag(records);
            else if (ParameterSetName == ParameterSet.LogicalOrTags)
                records = FilterResponseRecordsByLogicalOrTag(records);
            else
                throw new NotImplementedException($"Don't know how to process tags for parameter set '{ParameterSetName}'.");

            return base.PostProcessAdditionalFilters(records);
        }

        private IEnumerable<TObject> FilterResponseRecordsByLogicalAndTag(IEnumerable<TObject> records)
        {
            if (Tags != null)
            {
                //Select all records where all of the filter tags are present
                records = FilterTags(records, Tags, Enumerable.All);
            }

            return records;
        }

        private IEnumerable<TObject> FilterResponseRecordsByLogicalOrTag(IEnumerable<TObject> records)
        {
            if (Tag != null)
            {
                //Select all records where at least one of the filter tags is present
                records = FilterTags(records, Tag, Enumerable.Any);
            }

            return records;
        }

        private IEnumerable<TObject> FilterTags(IEnumerable<TObject> records, string[] tags, Func<IEnumerable<string>, Func<string, bool>, bool> action)
        {
            records = records.Where(record => record.Tags != null && action(tags, tag => HasTag(record, tag)));

            return records;
        }

        private bool HasTag(TObject record, string tag)
        {
            var wildcard = new WildcardPattern(tag, WildcardOptions.IgnoreCase);

            return record.Tags.Any(recordTag => wildcard.IsMatch(recordTag));
        }

        /// <summary>
        /// Retrieves an object that defines the dynamic parameters of this cmdlet.
        /// </summary>
        /// <returns>An object that defines the dynamic parameters of this cmdlet.</returns>
        public virtual object GetDynamicParameters() => GetDynamicParameters(ParameterSet.LogicalOrTags, ParameterSet.LogicalAndTags);
    }
}
