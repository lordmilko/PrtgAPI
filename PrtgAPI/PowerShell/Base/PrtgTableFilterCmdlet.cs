using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables that support enhanced record filters (sensors, devices, probes, etc).
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableFilterCmdlet<TObject, TParam> : PrtgTableCmdlet<TObject, TParam> where TParam : TableParameters<TObject> where TObject : ObjectTable
    {
        /// <summary>
        /// <para type="description">Retrieve an object with a specified ID.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Retrieve an obejct with a specified ID.")]
        public int[] Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableFilterCmdlet{TObject,TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="progressThreshold">The numeric threshold at which this cmdlet should show a progress bar when retrieving results.</param>
        public PrtgTableFilterCmdlet(Content content, int? progressThreshold) : base(content, progressThreshold)
        {
        }

        /// <summary>
        /// Processes additional parameters specific to the current cmdlet.
        /// </summary>
        protected override void ProcessAdditionalParameters()
        {
            ProcessIdFilter();

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
    }
}
