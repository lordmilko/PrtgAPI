using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

namespace PowerShell.TestAdapter
{
    public class HostUi : PSHostUserInterface
    {
        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        private void TryOutputProgress(string label, int percentage)
        {
            OutputProgress?.Invoke(label, percentage);
        }

        public Action<string, int> OutputProgress { get; set; }

        public Action<string> OutputString { get; set; }

        private void TryOutputString(string val)
        {
            OutputString?.Invoke(val);
        }

        public override void Write(string value)
        {
            TryOutputString(value);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            TryOutputString(value);
        }

        public override void WriteLine(string value)
        {
            TryOutputString(value + Environment.NewLine);
        }

        public override void WriteErrorLine(string value)
        {
            TryOutputString("[ERROR] " + value + Environment.NewLine);
        }

        public override void WriteDebugLine(string message)
        {
            TryOutputString("[DEBUG] " + message + Environment.NewLine);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            TryOutputProgress(record.Activity + " - " + record.StatusDescription, record.PercentComplete);
        }

        public override void WriteVerboseLine(string message)
        {
            TryOutputString("[VERBOSE] " + message + Environment.NewLine);
        }

        public override void WriteWarningLine(string message)
        {
            TryOutputString("[WARNING] " + message + Environment.NewLine);
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            return null;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return 0;
        }

        public override PSHostRawUserInterface RawUI => new RawHostUi();
    }
}
