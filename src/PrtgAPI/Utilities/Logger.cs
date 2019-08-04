using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace PrtgAPI.Utilities
{
    enum Indentation
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6
    }

    static class Logger
    {
        [Conditional("DEBUG")]
        public static void Log(string message, Indentation indentation = Indentation.Zero)
        {
            var original = Debug.IndentLevel;

            Debug.IndentLevel = (int) indentation * 2;

            Debug.WriteLine(message);

            Debug.IndentLevel = original;
        }

        [Conditional("DEBUG")]
        public static void Log(string message, Indentation indentation, IEnumerable<Expression> expressions)
        {
            Log(message, indentation);

            foreach (var expr in expressions)
                Log($"'{expr}'", (Indentation)((int)(indentation) + 1));
        }
    }
}