using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShell.TestAdapter
{
    static class StringUtilities
    {
        public static bool EqualsIgnoreCase(this string str, string other) => str.Equals(other, StringComparison.OrdinalIgnoreCase);
    }
}
