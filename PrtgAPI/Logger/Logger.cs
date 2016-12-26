using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    internal class Logger
    {
        private static bool debugEnabled;

        internal static bool DebugEnabled
        {
            get { return debugEnabled; }
            set
            {
#if DEBUG
                debugEnabled = value;
#else
                throw new NotSupportedException("Logging is not supported in Release builds");
#endif
            }
        }

        internal static void Debug(string message, bool newLine = true)
        {
            if (DebugEnabled)
            {
                if (newLine)
                    System.Diagnostics.Debug.WriteLine(message);
                else
                    System.Diagnostics.Debug.Write(false);
            }            
        }
    }
}
