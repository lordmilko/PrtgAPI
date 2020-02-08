using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Converters.Text
{
    /// <summary>
    /// Represents a line of a pretty printed tree.
    /// </summary>
    public class PrettyLine
    {
        /// <summary>
        /// Gets the node this line represents.
        /// </summary>
        public TreeNode Node { get; }

        /// <summary>
        /// Gets the complete text of this line.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrettyLine"/> class.
        /// </summary>
        /// <param name="node">The node this line represents.</param>
        /// <param name="text">The text of the line.</param>
        public PrettyLine(TreeNode node, string text)
        {
            Node = node;
            Text = text;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Text;
        }
    }
}
