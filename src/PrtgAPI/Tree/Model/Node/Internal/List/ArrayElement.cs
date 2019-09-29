using System.Diagnostics;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Wrap array values to prevent array covariance as per https://codeblog.jonskeet.uk/2013/06/22/array-covariance-not-just-ugly-but-slow-too/
    /// </summary>
    /// <typeparam name="T">The type of value to be wrapped.</typeparam>
    [DebuggerDisplay("{Value,nq}")]
    internal struct ArrayElement<T>
    {
        internal T Value;

        public static implicit operator T(ArrayElement<T> element)
        {
            return element.Value;
        }
    }
}
