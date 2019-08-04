using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Base class for all parameters that perform an action against a PRTG server pertaining to one or more objects in a single request.
    /// </summary>
    [ExcludeFromCodeCoverage]
    abstract class BaseMultiActionParameters : BaseParameters, IMultiTargetParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseActionParameters"/> class.
        /// </summary>
        /// <param name="objectIds">The IDs of the objects these parameters should apply to.</param>
        public BaseMultiActionParameters(int[] objectIds)
        {
            if (objectIds == null)
                throw new ArgumentNullException(nameof(objectIds));

            if (objectIds.Length == 0)
                throw new ArgumentException("At least one Object ID must be specified.", nameof(objectIds));

            ObjectIds = objectIds;
        }

        /// <summary>
        /// Gets or sets the IDs of the objects these parameters should apply to.
        /// </summary>
        public int[] ObjectIds
        {
            get { return (int[])this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }
    }
}
