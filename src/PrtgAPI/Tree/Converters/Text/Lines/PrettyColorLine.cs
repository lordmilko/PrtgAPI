using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Converters.Text
{
    /// <summary>
    /// Represents a colored line of a pretty printed tree.
    /// </summary>
    public class PrettyColorLine : PrettyLine
    {
        /// <summary>
        /// Gets the color that should be used for displaying the <see cref="Value"/>.
        /// </summary>
        public ConsoleColor? ValueColor { get; }

        /// <summary>
        /// Gets the branch symbols of this line. If this line is the root line, this value is null.
        /// </summary>
        public string Branch { get; }

        /// <summary>
        /// Gets the value contained in this line, without leading branch symbols.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrettyColorLine"/> class.
        /// </summary>
        /// <param name="valueColor">The color to use for the line's inner value.</param>
        /// <param name="text">The text of the line.</param>
        internal PrettyColorLine(ConsoleColor? valueColor, string text) : base(text)
        {
            var i = text.LastIndexOf(PrettyPrintConstants.ConnectChild);

            //If we're not the top level node, we'd like to split the record up so that we can color the node name without coloring the tree line
            if (i > 0)
            {
                Branch = text.Substring(0, i + 1);

                //We have to extract the name from the record instead of maybe passing
                //a TreeNode to this constructor as GetName() may have returned a custom
                //value for cases where the node difference includes TreeNodeDifference.Name
                Value = text.Substring(i + 1);
            }
            else
                Value = text;

            ValueColor = valueColor;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (ValueColor == null)
                return Text;
            else
                return $"{Text} ({ValueColor})";
        }
    }
}
