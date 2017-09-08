using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Attributes
{
    /// <summary>
    /// Specifies that an enum represents a set of numeric values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    sealed class NumericEnumAttribute : Attribute
    {
    }
}
