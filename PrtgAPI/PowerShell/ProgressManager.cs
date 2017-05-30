using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.Helpers;

namespace PrtgAPI.PowerShell
{
    internal class ProgressManager : IDisposable
    {
        private static Stack<ProgressRecord> progressRecords = new Stack<ProgressRecord>();

        private const string DefaultActivity = "Activity";
        private const string DefaultDescription = "Description";

        public ProgressRecord CurrentRecord => progressRecords.Peek();

        public bool ContainsProgress => CurrentRecord.Activity != DefaultActivity && (CurrentRecord.StatusDescription != DefaultDescription || InitialDescription != string.Empty);

        public bool PreviousContainsProgress => PreviousRecord?.Activity != DefaultActivity && PreviousRecord?.StatusDescription != DefaultDescription;

        public bool FirstInChain => pipeToPrtgCmdlet && progressRecords.Count == 1;

        public bool PartOfChain => pipeToPrtgCmdlet || progressRecords.Count > 1;

        private bool pipeToPrtgCmdlet => cmdlet.MyInvocation.MyCommand.ModuleName == cmdlet.CommandRuntime.GetDownstreamCmdlet()?.ModuleName;

        public bool LastInChain => !pipeToPrtgCmdlet;

        public string InitialDescription { get; set; }

        public int? TotalRecords { get; set; }

        public ProgressRecord PreviousRecord => progressRecords.Skip(1).FirstOrDefault();

        private PSCmdlet cmdlet;

        private int recordsProcessed = -1;

        public ProgressManager(PSCmdlet cmdlet)
        {
            progressRecords.Push(new ProgressRecord(progressRecords.Count + 1, "Activity", "Description"));

            if (PreviousRecord != null)
                CurrentRecord.ParentActivityId = PreviousRecord.ActivityId;

            this.cmdlet = cmdlet;
        }

        ~ProgressManager()
        {
            Dispose(false);
        }

        #region IDisposable

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                progressRecords.Pop();
            }
        }

        #endregion

        public void RemovePreviousOperation()
        {
            if (PreviousRecord != null && PreviousRecord.CurrentOperation != null)
            {
                PreviousRecord.CurrentOperation = null;

                WriteProgress(PreviousRecord);
            }
        }

        private void WriteProgress()
        {
            WriteProgress(CurrentRecord);
        }

        public void WriteProgress(string activity, string statusDescription)
        {
            CurrentRecord.Activity = activity;
            CurrentRecord.StatusDescription = statusDescription;

            WriteProgress();
        }

        private void WriteProgress(ProgressRecord progressRecord)
        {
            if (progressRecord.Activity == DefaultActivity || progressRecord.StatusDescription == DefaultDescription)
                throw new InvalidOperationException("Attempted to write progress on an uninitialized ProgressRecord. If this is a Release build, please report this bug along with the cmdlet chain you tried to execute. To disable PrtgAPI Cmdlet Progress in the meantime use Disable-PrtgProgress");

            if (PreviousRecord == null)
                cmdlet.WriteProgress(progressRecord);
            else
            {
                var sourceId = GetLastSourceId(cmdlet.CommandRuntime);

                cmdlet.CommandRuntime.WriteProgress(sourceId, progressRecord);
            }
        }

        internal static long GetLastSourceId(ICommandRuntime commandRuntime)
        {
            return Convert.ToInt64(commandRuntime.GetType().GetField("_lastUsedSourceId", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));
        }

        public void CompleteProgress()
        {
            InitialDescription = null;
            TotalRecords = null;
            recordsProcessed = -1;

            CurrentRecord.RecordType = ProgressRecordType.Completed;

            WriteProgress();

            CurrentRecord.Activity = DefaultActivity;
            CurrentRecord.StatusDescription = DefaultDescription;
            CurrentRecord.RecordType = ProgressRecordType.Processing;
        }

        public void UpdateRecordsProcessed()
        {
            recordsProcessed++;

            CurrentRecord.StatusDescription = $"{InitialDescription} {recordsProcessed}/{TotalRecords}";

            if(recordsProcessed > 0)
                CurrentRecord.PercentComplete = (int)(recordsProcessed / Convert.ToDouble(TotalRecords) * 100);

            WriteProgress();
        }

        public void TrySetPreviousOperation(string operation)
        {
            if (PreviousRecord != null)
                SetPreviousOperation(operation);
        }

        public void SetPreviousOperation(string operation)
        {
            PreviousRecord.CurrentOperation = operation;

            WriteProgress(PreviousRecord);
        }

        public void DisplayInitialProgress()
        {
            CurrentRecord.StatusDescription = InitialDescription;

            WriteProgress();
        }

        public void TryOverwritePreviousOperation(string activity, string progressMessage)
        {
            if (PreviousRecord != null)
            {
                PreviousRecord.Activity = activity;

                var p = PreviousRecord.StatusDescription.Substring(PreviousRecord.StatusDescription.LastIndexOf(" ") + 1);

                PreviousRecord.StatusDescription = $"{progressMessage} ({p})";

                WriteProgress(PreviousRecord);
            }
        }
    }
}
