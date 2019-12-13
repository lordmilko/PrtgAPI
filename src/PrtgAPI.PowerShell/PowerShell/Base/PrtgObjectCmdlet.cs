using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.PowerShell.Progress;
using PrtgAPI.Utilities;

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
        protected PrtgObjectCmdlet() : base(IObjectExtensions.GetTypeDescription(typeof(T)))
        {
        }

        /// <summary>
        /// Retrieves all records of a specified type from a PRTG Server. Implementors can call different methods of a
        /// <see cref="PrtgClient"/> based on the type of object they wish to retrieve.
        /// </summary>
        /// <returns>A list of records relevant to the caller.</returns>
        protected abstract IEnumerable<T> GetRecords();

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            IEnumerable<T> records;

            if (ProgressManager.GetRecordsWithVariableProgress)
                records = GetResultsWithVariableProgress(GetRecords);
            else if (ProgressManager.GetResultsWithProgress)
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

            return UpdatePreviousProgressAndGetCount(records, count);
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
                    ProgressManager.SetPreviousOperation($"Retrieving all {TypeDescription.ToLower().ForcePlural()}");
                }
            }

            return count;
        }

        private IEnumerable<T> UpdatePreviousProgressAndGetCount(IEnumerable<T> records, int count)
        {
            records = GetCount(records, ref count);

            UpdatePreviousProgressAndSetCount(count);

            return records;
        }

        /// <summary>
        /// Display the initial progress message for the first cmdlet in the chain.<para/>
        /// Returns an integer so that overridden instances of this method may support progress scenarios such as streaming
        /// (where a "detecting total number of items" message is displayed before requesting the object totals.
        /// </summary>
        /// <returns>-1. Override this method in a derived class to optionally return the total number of objects that will be retrieved.</returns>
        protected virtual int DisplayFirstInChainMessage()
        {
            int count = -1;

            SetObjectSearchProgress(ProcessingOperation.Retrieving);

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
        /// Filter a response with a wildcard expression on a specified property.
        /// </summary>
        /// <param name="records">The records to filter.</param>
        /// <param name="pattern">The wildcard expression to filter with.</param>
        /// <param name="getProperty">A function that yields the property to filter by.</param>
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

        /// <summary>
        /// Filter a response on a specified property.
        /// </summary>
        /// <typeparam name="TObject">The type of object to filter.</typeparam>
        /// <typeparam name="TProperty">The type of property to filter on.</typeparam>
        /// <param name="filters">A list of values to filter based on.</param>
        /// <param name="getProperty">A function that yields the property to filter by.</param>
        /// <param name="records">The records to filter.</param>
        /// <returns>A list of records that matched any of the specified filters.</returns>
        protected IEnumerable<TObject> FilterResponseRecords<TObject, TProperty>(TProperty[] filters, Func<TObject, TProperty> getProperty, IEnumerable<TObject> records)
        {
            if (filters != null)
                records = records.Where(r => filters.Contains(getProperty(r)));

            return records;
        }

        /// <summary>
        /// Filter a response by either a wildcard expression or an object contained in a <see cref="NameOrObject{T}"/> value.
        /// </summary>
        /// <typeparam name="TProperty">The type of property to filter on.</typeparam>
        /// <param name="filters">A list of values to filter based on.</param>
        /// <param name="getProperty">A function that yields the property to filter by.</param>
        /// <param name="records">The records to filter.</param>
        /// <returns>A list of records that matched any of the specified filters.</returns>
        protected IEnumerable<T> FilterResponseRecordsByPropertyNameOrObjectId<TProperty>(
            NameOrObject<TProperty>[] filters,
            Func<T, TProperty> getProperty,
            IEnumerable<T> records) where TProperty : PrtgObject
        {
            if (filters == null)
                return records;

            return records.Where(
                record =>
                {
                    return filters.Any(filter =>
                    {
                        var action = getProperty(record);

                        if (action == null)
                            return false;

                        if (filter.IsObject)
                            return action.Id == filter.Object.Id;
                        else
                        {
                            var wildcard = new WildcardPattern(filter.Name, WildcardOptions.IgnoreCase);

                            return wildcard.IsMatch(action.Name);
                        }
                    });
                }
            );
        }

        /// <summary>
        /// Filter records returned from PRTG by one or more wildcards.
        /// </summary>
        /// <param name="arr">The array of wildcards to filter against.</param>
        /// <param name="getProperty">A function that yields the property to filter by.</param>
        /// <param name="records">The records to filter.</param>
        /// <returns>A collection of filtered records.</returns>
        protected IEnumerable<T> FilterResponseRecordsByWildcardArray(string[] arr, Func<T, string> getProperty, IEnumerable<T> records)
        {
            if (arr != null)
            {
                records = records.Where(
                    record => arr
                        .Select(a => new WildcardPattern(a, WildcardOptions.IgnoreCase))
                        .Any(filter => filter.IsMatch(getProperty(record))
                    )
                );
            }

            return records;
        }
    }
}
