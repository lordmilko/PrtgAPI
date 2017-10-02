using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all cmdlets that return a list of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects that will be retrieved.</typeparam>
    public abstract class PrtgObjectCmdlet<T> : PrtgProgressCmdlet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgObjectCmdlet{T}"/> class.
        /// </summary>
        public PrtgObjectCmdlet() : base(GetTypeDescription(typeof(T)))
        {
        }

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

            if (ProgressManager.PipeFromVariableWithProgress && PrtgSessionState.EnableProgress)
                records = GetResultsWithVariableProgress(GetRecords);
            else if (ProgressManager.PartOfChain && PrtgSessionState.EnableProgress)
                records = GetResultsWithProgress(GetRecords);
            else
                records = GetRecords();

            WriteList(records);
        }

        /// <summary>
        /// Retrieve results from a PRTG server while displaying progress for objects that have been piped from a variable.
        /// </summary>
        /// <param name="getResults">The function to execute to retrieve this cmdlet's results.</param>
        /// <returns>A collection of objects returned from a PRTG Server. that match the specified search criteria.</returns>
        protected IEnumerable<T> GetResultsWithVariableProgress(Func<IEnumerable<T>> getResults)
        {
            UpdatePreviousAndCurrentVariableProgressOperations();

            var records = getResults().ToList();

            ProgressManager.TotalRecords = records.Count;

            return records;
        }

        /// <summary>
        /// Retrieve results from a PRTG server while displaying progress for objects that have been piped between cmdlets.
        /// </summary>
        /// <param name="getResults">The function to execute to retrieve this cmdlet's results.</param>
        /// <returns>A collection of objects returned from a PRTG Server. that match the specified search criteria.</returns>
        protected IEnumerable<T> GetResultsWithProgress(Func<IEnumerable<T>> getResults)
        {
            //May return the count of records we'll be retrieving if we're streaming (for example)
            int count = DisplayInitialProgress();

            var records = getResults();

            return UpdatePreviousProgressCount(records, count);
        }

        private int DisplayInitialProgress()
        {
            int count = -1;

            if (ProgressManager.FirstInChain)
            {
                //If DisplayFirstInChainMessage is overridden, count _may_ be set (e.g. if we are streaming records,
                //in which case we will get the total number of records we _will_ retrieve before returning)
                count = DisplayFirstInChainMessage();
            }
            else
            {
                if (ProgressManager.PreviousContainsProgress)
                {
                    ProgressManager.SetPreviousOperation($"Retrieving all {TypeDescription.ToLower()}s");
                }
            }

            return count;
        }

        private IEnumerable<T> UpdatePreviousProgressCount(IEnumerable<T> records, int count)
        {
            records = GetCount(records, ref count);

            ProgressManager.TotalRecords = count;

            if (!ProgressManager.LastInChain)
            {
                SetObjectSearchProgress(ProcessingOperation.Processing, count);
            }

            return records;
        }

        /// <summary>
        /// Display the initial progress message for the first cmdlet in the chain.<para/>
        /// Returns an integer so that overridden instances of this method may support progress scenarios such as streaming (where a "detecting total number of items" message is displayed before requesting the object totals.
        /// </summary>
        /// <returns>-1. Override this method in a derived class to optionally return the total number of objects that will be retrieved.</returns>
        protected virtual int DisplayFirstInChainMessage()
        {
            int count = -1;

            SetObjectSearchProgress(ProcessingOperation.Retrieving, null);

            ProgressManager.DisplayInitialProgress();

            return count;
        }

        /// <summary>
        /// Retrieves the number of elements returned from a request. Generally the collection of records should internally be a <see cref="List{T}"/>.<para/>
        /// For scenarios where this is not the case, this method can be overridden in derived classes.
        /// </summary>
        /// <param name="records">The collection of records to count.Should be a <see cref="List{T}"/></param>
        /// <param name="count">The count of records to be returned from this method.</param>
        /// <returns></returns>
        protected virtual IEnumerable<T> GetCount(IEnumerable<T> records, ref int count)
        {
            var list = records as List<T> ?? records.ToList();

            count = list.Count;

            return list;
        }

        

        /// <summary>
        /// Writes a list to the output pipeline.
        /// </summary>
        /// <param name="sendToPipeline">The list that will be output to the pipeline.</param>
        internal void WriteList(IEnumerable<T> sendToPipeline)
        {
            PreUpdateProgress();

            foreach (var item in sendToPipeline)
            {
                DuringUpdateProgress();

                WriteObject(item);
            }

            PostUpdateProgress();
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
