using System;
using System.Management.Automation;
using PrtgAPI.Tree.Converters.Text.Writers;

namespace PrtgAPI.PowerShell.Tree
{
    /// <summary>
    /// Represents a writer capable of printing a collection of pretty lines to the PowerShell console.
    /// </summary>
    internal class PowerShellPrettyColorWriter : PrettyColorWriter
    {
        protected PSCmdlet cmdlet;

        private bool color;

        internal PowerShellPrettyColorWriter(PSCmdlet cmdlet, bool color = true)
        {
            this.cmdlet = cmdlet;
            this.color = color;
        }

        protected override void Write(string value)
        {
            cmdlet.Host.UI.Write(value);
        }

        protected override void WriteLine(ConsoleColor? valueColor, string value)
        {
            if (valueColor == null || !color)
                cmdlet.Host.UI.WriteLine(value);
            else
                cmdlet.Host.UI.WriteLine(valueColor.Value, Console.BackgroundColor, value);
        }

        //We'd like to have a one line gap before and after the printed tree
        protected override void WriteHeader() => cmdlet.Host.UI.WriteLine();
        protected override void WriteFooter() => cmdlet.Host.UI.WriteLine();
    }
}
