using System.Management.Automation;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return advanced objects found in tables that support enhanced record filters (sensors, devices, probes, etc).
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableFilterCmdlet<TObject, TParam> : PrtgTableCmdlet<TObject, TParam>
        where TObject : ITableObject, IObject
        where TParam : TableParameters<TObject>
    {
        /// <summary>
        /// <para type="description">Retrieve an object with a specified ID.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Retrieve an object with a specified ID.")]
        public int[] Id { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableFilterCmdlet{TObject,TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="shouldStream">Whether this cmdlet should have streaming enabled.</param>
        public PrtgTableFilterCmdlet(Content content, bool? shouldStream) : base(content, shouldStream)
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
