using System;
using System.Collections.Generic;

namespace PrtgAPI.Tree.Converters.Text.Writers
{
    /// <summary>
    /// Represents a writer capable of printing a collection of colored pretty lines.
    /// </summary>
    public abstract class PrettyColorWriter : PrettyWriter
    {
        /// <summary>
        /// Writes the specified string value to the printer's output destination.
        /// </summary>
        /// <param name="value">The value to write.</param>
        protected abstract void Write(string value);

        /// <summary>
        /// Writes the specified string value with optional coloring to the printer's output destination,
        /// followed by a line terminator or line termination action.
        /// </summary>
        /// <param name="valueColor">The color to use for the <paramref name="value"/>. If this value is null, no color should be applied.</param>
        /// <param name="value">The value to write.</param>
        protected abstract void WriteLine(ConsoleColor? valueColor, string value);

        /// <summary>
        /// Processes all pretty lines.
        /// </summary>
        /// <param name="lines">The lines to process.</param>
        public override void Execute(List<PrettyLine> lines)
        {
            if (lines.Count > 0)
                WriteHeader();

            foreach (PrettyColorLine line in lines)
            {
                if (line.Branch != null)
                {
                    Write(line.Branch);
                    WriteLine(line.ValueColor, line.Value);
                }
                else
                {
                    WriteLine(line.ValueColor, line.Text);
                }
            }

            if (lines.Count > 0)
                WriteFooter();
        }
    }
}
