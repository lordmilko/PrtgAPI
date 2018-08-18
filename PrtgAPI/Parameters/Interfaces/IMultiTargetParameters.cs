using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> capable of targeting multiple objects in a single request.
    /// </summary>
    interface IMultiTargetParameters : IParameters
    {
        int[] ObjectIds { get; set; }
    }
}
