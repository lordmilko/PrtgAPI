using PrtgAPI.Objects.Shared;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a schedule used to indicate when monitoring should be active on an object.
    /// </summary>
    public class Schedule : PrtgObject
    {

        internal Schedule(string raw) : base(raw)
        {
        }
    }
}
