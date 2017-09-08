using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies that a value should be used as-is, without further processing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class LiteralValueAttribute : Attribute
    {
    }
}
