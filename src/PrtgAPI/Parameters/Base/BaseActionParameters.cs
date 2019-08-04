using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Base class for all parameters that perform an action against a PRTG server pertaining to a specific object.
    /// </summary>
    [ExcludeFromCodeCoverage]
    class BaseActionParameters : BaseParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseActionParameters"/> class.
        /// </summary>
        /// <param name="objectOrId">The object or ID of the object these parameters should apply to.</param>
        public BaseActionParameters(Either<IPrtgObject, int> objectOrId) : this(objectOrId.GetId())
        {
        }

        internal BaseActionParameters(int objectId)
        {
            ObjectId = objectId;
        }

        /// <summary>
        /// Gets or sets the ID of the object these parameters should apply to.
        /// </summary>
        public int ObjectId
        {
            get { return (int) this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }
    }
}
