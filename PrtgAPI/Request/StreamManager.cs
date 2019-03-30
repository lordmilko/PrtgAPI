using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request
{
    class StreamManager<TObject, TParam> where TParam : PageableParameters, IXmlParameters
    {
        /// <summary>
        /// Total number of objects that exist on the server.
        /// </summary>
        public int? TotalExist { get; set; }

        /// <summary>
        /// Total number of objects requested in the <see cref="PageableParameters"/>.
        /// </summary>
        public int? TotalRequested => SourceParameters.Count;

        /// <summary>
        /// Maximum number of items that were requested that actually exist.
        /// </summary>
        public int? TotalRequestedThatExist { get; set; }

        /// <summary>
        /// Start position specified in the <see cref="PageableParameters"/>.
        /// </summary>
        public int RequestedStartPosition => SourceParameters.Start ?? 0;

        /// <summary>
        /// Total number of objects to retrieve after adjusting the <see cref="TotalRequested"/> and
        /// <see cref="RequestedStartPosition"/> according to the <see cref="TotalExist"/>.
        /// </summary>
        public int? TotalToRetrieve
        {
            get
            {
                if (HasStart && RequestedSkippedRecords)
                {
                    return Math.Max(TotalRequestedThatExist.Value - RequestedStartPosition, 0);
                }

                return TotalRequestedThatExist;
            }
        }

        public bool HasStart => SourceParameters.Start != null && SourceParameters.Start != SourceParameters.StartOffset;

        /// <summary>
        /// Indicates whether there are not enough free records remaining to fulfil the specified <see cref="TotalRequested"/>,
        /// causing PRTG to also include records inside the <see cref="RequestedStartPosition"/>.
        /// </summary>
        public bool RequestedSkippedRecords
        {
            get
            {
                if (TotalRequested == null && HasStart)
                    return true;

                if (TotalRequestedThatExist + RequestedStartPosition > TotalExist)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// A clone of the <see cref="SourceParameters"/> to use for executing the request.
        /// </summary>
        public TParam RequestParameters { get; private set; }

        /// <summary>
        /// The original request parameters that were passed to a <see cref="PrtgClient"/>.
        /// </summary>
        public TParam SourceParameters { get; }

        #region Request Index

        public int RequestIndex { get; set; }

        public int StartIndex
        {
            get
            {
                var index = SourceParameters.Start ?? 0;

                if (index == TotalRequestedThatExist)
                    index = 0;

                return index;
            }
        }

        public int EndIndex
        {
            get
            {
                if (TotalExist != null)
                {
                    /* Requested 100. 50 exist.  Start at 3. Min(53,  100) = Last Index is 53. Range is 0 - 53
                     * Requested 100. 100 exist. Start at 3. Min(103, 100) = Last Index is 100. Range is 0 - 100
                     * Requested 100. 50 exist.  Start at 3+1. Min(53, 100) + 1 = Last Index is 54. Range is 1-54
                     * Requested 100. 100 exist. Start at 3+1. Min(103, 100) + 1 = Last Index is 101. Range is 1-101
                     */

                     return Math.Min(TotalRequestedThatExist.Value + RequestedStartPosition - SourceParameters.StartOffset, TotalExist.Value) + SourceParameters.StartOffset;
                }

                return TotalRequestedThatExist.Value + RequestedStartPosition;
            }
        }

        public void UpdateRequestIndex()
        {
            Logger.Log("Incrementing stream page");
            Logger.Log($"Current page: {RequestParameters.Page} (Start: {RequestParameters.Start}, Count: {RequestParameters.Count}, PageSize: {RequestParameters.PageSize}, Start Offset: {RequestedStartPosition})", Indentation.One);

            RequestParameters.Page++;

            Logger.Log($"New page: {RequestParameters.Page} (Start: {RequestParameters.Start}, Count: {RequestParameters.Count}, PageSize: {RequestParameters.PageSize}, Start Offset: {RequestedStartPosition})", Indentation.One);

            RequestIndex = RequestParameters.Start.Value;

            var remaining = EndIndex - RequestIndex;

            if (remaining < RequestParameters.Count)
                RequestParameters.Count = remaining;

            Logger.Log($"Index: {RequestIndex}. Remaining: {remaining}. Count for this request: {RequestParameters.Count}", Indentation.One);
        }

        #endregion

        public int CountPerRequest
        {
            get
            {
                Logger.Log("Calculating number of records to retrieve per request");

                if (HasStart)
                    Debug.Assert(TotalExist != null);

                int count;

                if (TotalRequestedThatExist == null)
                {
                    Logger.Log("TotalRequestedThatExist is null. Either TotalRequested or TotalExist is unknown", Indentation.One);

                    //We don't know TotalExist

                    //If a Count was specified (e.g. 501) we might shrink it to only PageSize records per request.
                    //If a Count wasn't specified, we're requesting unlimited, so default to PageSize per request
                    if (RequestParameters.Count != null)
                        count = Math.Min(RequestParameters.PageSize, RequestParameters.Count.Value);
                    else
                        count = RequestParameters.PageSize;

                    Logger.Log($"We will retrieve {count} records per request", Indentation.One);
                }
                else
                {
                    Logger.Log("TotalRequestedThatExist is not null. TotalExist must be known", Indentation.One);

                    //We have a Count, we know TotalExist, and we know the most we could possibly retrieve. As above,
                    //how many do we need to retrieve per request?
                    count = Math.Min(RequestParameters.PageSize, TotalRequestedThatExist.Value);

                    Logger.Log($"Planned count is {count}", Indentation.One);

                    //Given the amount we KNOW we can retrieve, how many CAN we retrieve without overlapping skipped records?
                    if (HasStart && RequestedSkippedRecords)
                    {
                        Logger.Log("Originally requested count will retrieve skipped records. Checking whether skipped records will be retrieved in this request", Indentation.One);
                        Logger.Log($"Proposed request index: {RequestIndex}, Exist: {TotalExist}, Start: {RequestedStartPosition}", Indentation.One);

                        //Are we requesting skipped records in THIS request?

                        //10 > (10 - 3): Need to request 7 instead then
                        if (count > TotalExist - RequestedStartPosition)
                        {
                            //0 based start: 0-2 are the first 3 records. We want the remaining 7, starting at index 3
                            //1 based start: 1-2 are the first 2 records. We want the remaining 8, starting at index 3. So add StartOffset
                            count = TotalExist.Value - RequestedStartPosition + SourceParameters.StartOffset;

                            Logger.Log($"Adjusted count to {count} as count overlaps with skipped records", Indentation.Two);

                            if (count < 0)
                            {
                                count = 0;

                                Logger.Log($"Adjusted count to {count} as previous adjusted count was below 0", Indentation.Two);
                            }
                        }
                        else
                            Logger.Log("Skipped records will not be overlapped", Indentation.Two);
                    }
                }

                return count;
            }
        }

        Func<TParam, Tuple<List<TObject>, int>> getObjectsFunc;

        public Func<TParam, Tuple<List<TObject>, int>> GetObjectsFunc
        {
            get
            {
                if (getObjectsFunc == null)
                    getObjectsFunc = p =>
                    {
                        var raw = engine.GetObjectsRaw<TObject>(p, validateValueTypes: ValidateValueTypes);
                        return Tuple.Create(raw.Items, raw.TotalCount);
                    };

                return getObjectsFunc;
            }
            set { getObjectsFunc = value; }
        }

        Func<TParam, Task<List<TObject>>> getObjectsAsyncFunc;

        public Func<TParam, Task<List<TObject>>> GetObjectsAsyncFunc
        {
            get
            {
                if (getObjectsAsyncFunc == null)
                    getObjectsAsyncFunc = p => engine.GetObjectsAsync<TObject>(p, validateValueTypes: ValidateValueTypes);

                return getObjectsAsyncFunc;
            }
            set { getObjectsAsyncFunc = value; }
        }

        public Tuple<List<TObject>, int> GetObjects() => GetObjectsFunc(RequestParameters);

        public async Task<List<TObject>> GetObjectsAsync() => await GetObjectsAsyncFunc(RequestParameters).ConfigureAwait(false);

        public bool Serial { get; }

        public bool DirectCall { get; }

        public bool ValidateValueTypes { get; }

        ObjectEngine engine;

        #region Constructors

        public StreamManager(ObjectEngine engine, TParam parameters)
        {
            this.engine = engine;
            SourceParameters = parameters;
        }

        public StreamManager(ObjectEngine engine, TParam parameters, Func<int> getCount, bool serial) : this(engine, parameters)
        {
            TotalExist = GetMandatoryTotalExist(getCount, serial);
            TotalRequestedThatExist = GetTotalRequestedThatExist();
            Serial = serial;
        }

        public StreamManager(
            ObjectEngine engine,
            TParam parameters,
            Func<int> getCount,
            bool serial,
            bool directCall,
            Func<TParam, Tuple<List<TObject>, int>> getObjects,
            Func<TParam, Task<List<TObject>>> getObjectsAsync = null,
            bool validateValueTypes = true) : this(engine, parameters, getCount, serial)
        {
            DirectCall = directCall;
            GetObjectsFunc = getObjects;
            GetObjectsAsyncFunc = getObjectsAsync;
            ValidateValueTypes = validateValueTypes;
        }

        #endregion
        #region Constructor Initialization

        private int? GetMandatoryTotalExist(Func<int> getCount, bool serial)
        {
            int? total = null;

            //If a start offset has been specified, asking for too many sensors may eat
            //into the records we skipped, effectively ignoring the start offset. To prevent this,
            //we need to adjust the number of records we ask for so that start + count <= totalObjects.
            if (HasStart || !serial)
            {
                Logger.Log($"Start value of '{SourceParameters.Start}' was specified or request is not serial. Retrieving total available");
                total = getCount();

                Logger.Log($"Server responded that {total} objects exist");
            }

            return total;
        }

        private int? GetTotalRequestedThatExist()
        {
            int? total = null;

            //If count < total, only get total many objects. If count > total, there
            //are only total available, so we can only get that many.
            if (SourceParameters.Count != null && TotalExist != null)
            {
                Logger.Log($"Count of {SourceParameters.Count} was specified and TotalExist is known. Calculating Total Requested that actually exist");
                total = Math.Min(SourceParameters.Count.Value, TotalExist.Value);
                Logger.Log($"{total} objects will be requested (before factoring in start index)");
            }
            else
                total = TotalExist;

            return total;
        }

        public void InitializeRequest()
        {
            RequestIndex = StartIndex;

            RequestParameters = ((IShallowCloneable<TParam>)SourceParameters).ShallowClone();
            RequestParameters.Count = CountPerRequest;

            Logger.Log($"Requesting {RequestParameters.Count} objects in first request");
        }

        public void UpdateTotals(int total)
        {
            if (TotalExist == null)
            {
                Logger.Log($"TotalExist is now known to be {total}");
                TotalExist = total;
                TotalRequestedThatExist = GetTotalRequestedThatExist();

                Logger.Log($"TotalRequestedThatExist is now known to be {TotalRequestedThatExist}");
            }
        }

        public bool StreamEnded(List<TObject> items)
        {
            //Some object types (such as Logs) lie about their total number of objects.
            //If no objects are returned, we've reached the total number of items
            if (items.Count == 0)
                return true;

            return false;
        }

        #endregion
    }
}
