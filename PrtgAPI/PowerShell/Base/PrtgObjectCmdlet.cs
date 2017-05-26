using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return a list of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects that will be retrieved.</typeparam>
    public abstract class PrtgObjectCmdlet<T> : PrtgCmdlet
    {
        protected enum ProcessingOperation
        {
            Retrieving,
            Processing
        };

        /// <summary>
        /// Retrieves all records of a specified type from a PRTG Server. Implementors can call different methods of a <see cref="PrtgClient"/> based on the type they wish to retrieve.
        /// </summary>
        /// <returns>A list of records relevant to the caller.</returns>
        protected abstract IEnumerable<T> GetRecords();

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            IEnumerable<T> records = null;

            if (ProgressManager.PartOfChain && !PrtgSessionState.DisableProgress)
                records = GetResultsWithProgress(GetRecords);
            else
                records = GetRecords();

            WriteList(records);
        }

        protected IEnumerable<T> GetResultsWithProgress(Func<IEnumerable<T>> getResults)
        {
            //when we're piping from a variable, we'd like to tell the user how many items we're processing

            if (ProgressManager.FirstInChain)
            {
                SetObjectSearchProgress(ProcessingOperation.Retrieving, null);

                ProgressManager.DisplayInitialProgress();
            }
            else
            {
                if (ProgressManager.PreviousContainsProgress)
                {
                    ProgressManager.SetPreviousOperation($"Retrieving all {typeof(T).Name.ToLower()}s");
                }
            }

            var records = getResults().ToList();

            if (ProgressManager.PreviousContainsProgress)
            {
                ProgressManager.TotalRecords = records.Count;

                if (!ProgressManager.LastInChain)
                {
                    SetObjectSearchProgress(ProcessingOperation.Processing, records.Count);
                }
            }

            return records.Select(s => s);
        }

        protected void SetObjectSearchProgress(ProcessingOperation operation, int? count)
        {
            ProgressManager.CurrentRecord.Activity = $"PRTG {typeof(T).Name} Search";

            if (operation == ProcessingOperation.Processing)
                ProgressManager.InitialDescription = $"Processing {typeof (T).Name.ToLower()}";
            else
                ProgressManager.InitialDescription = $"Retrieving all {typeof (T).Name.ToLower()}s";

            if (count != null)
                ProgressManager.TotalRecords = count;
        }

        /// <summary>
        /// Writes a list to the output pipeline.
        /// </summary>
        /// <typeparam name="T">The type of the list that will be output.</typeparam>
        /// <param name="sendToPipeline">The list that will be output to the pipeline.</param>
        /// <param name="progressManager"></param>
        internal void WriteList<T>(IEnumerable<T> sendToPipeline)
        {
            if (ProgressManager.ContainsProgress)
            {
                ProgressManager.RemovePreviousOperation();
                ProgressManager.UpdateRecordsProcessed();
            }

            foreach (var item in sendToPipeline)
            {
                if (ProgressManager.ContainsProgress)
                {
                    ProgressManager.UpdateRecordsProcessed();
                }

                WriteObject(item);
            }

            if (ProgressManager.ContainsProgress)
                ProgressManager.CompleteProgress();
        }

        /// <summary>
        /// Filter a response with a wildcard expression on a specified property.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <param name="pattern">The wildcard expression to filter with.</param>
        /// <param name="getProperty">A function that yields the property to filter on.</param>
        /// <returns>A list of records that match the specified filter.</returns>
        protected IEnumerable<T> FilterResponseRecords(IEnumerable<T> records, string pattern, Func<T, string> getProperty)
        {
            if (pattern != null)
            {
                var filter = new WildcardPattern(pattern.ToLower());
                records = records.Where(r => filter.IsMatch(getProperty(r).ToLower()));
            }

            return records;
        }
    }
}
