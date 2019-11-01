using System.Collections.Generic;

namespace PrtgAPI.Tree.Converters.Text.Writers
{
    /// <summary>
    /// Represents a writer capable of printing a collection of pretty lines.
    /// </summary>
    public abstract class PrettyWriter
    {
        /// <summary>
        /// Writes a header to the output stream before any pretty lines are written.
        /// </summary>
        protected virtual void WriteHeader()
        {
        }

        /// <summary>
        /// Writes a footer to the output stream after all pretty lines are written.
        /// </summary>
        protected virtual void WriteFooter()
        {
        }

        /// <summary>
        /// Processes all pretty lines.
        /// </summary>
        /// <param name="lines">The lines to process.</param>
        public abstract void Execute(List<PrettyLine> lines);
    }
}
