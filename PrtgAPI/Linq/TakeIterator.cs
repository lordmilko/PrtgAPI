using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq
{
    class TakeIterator<TObject, TParam> : IEnumerator<TObject>, IEnumerable<TObject> where TParam : PageableParameters
    {
        private int takeCount;
        private int taken;

        private int initialStart;
        private int initialPageSize;
        private Func<TParam, Func<int>, IEnumerable<TObject>> streamer;
        private Func<int> getCount;

        private int totalExist = -1;

        private Func<IEnumerable<TObject>, IEnumerable<TObject>> postProcessor;
        private Func<IEnumerable<TObject>, IEnumerable<TObject>> postProcessorSorter;
        private TParam parameters;
        private IEnumerator<TObject> enumerator;
        private bool stream;

        private int processed;

        private int attempts;

        private IEnumerable<TObject> currentResponse;

        private int? recordCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeIterator{TObject, TParam}"/> class.
        /// </summary>
        /// <param name="takeCount">The desired number of objects to retrieve.</param>
        /// <param name="parameters">The parameters to use for the request.</param>
        /// <param name="streamer">A function that retrieves results from the server.</param>
        /// <param name="getCount">A function that retrieves the total number of records that match the specified parameters.</param> 
        /// <param name="postProcessor">A post processing function that filters the retrieved results.</param>
        /// <param name="postProcessorSorter">A post processing sorting function that sorts the taken records. Only applies if <paramref name="stream"/> is false.</param>
        /// <param name="stream">Whether to stream the records across multiple requests or request all of the objects at once.</param>
        public TakeIterator(
            int takeCount,
            TParam parameters,
            Func<TParam, Func<int>, IEnumerable<TObject>> streamer,
            Func<int> getCount,
            Func<IEnumerable<TObject>, IEnumerable<TObject>> postProcessor,
            Func<IEnumerable<TObject>, IEnumerable<TObject>> postProcessorSorter = null,
            bool stream = true
        )
        {
            this.takeCount = takeCount;
            this.parameters = parameters;
            this.streamer = streamer;
            this.getCount = getCount;
            this.postProcessor = postProcessor;
            this.postProcessorSorter = postProcessorSorter;
            this.stream = stream;

            initialStart = parameters.Start ?? 0;
            initialPageSize = parameters.PageSize;
        }

        public void Dispose()
        {
            Current = default(TObject);
        }

        public bool MoveNext()
        {
            if (stream)
                return StreamMoveNext();

            return FixedMoveNext();
        }

        private bool StreamMoveNext()
        {
            Logger.Log("Called StreamMoveNext", Indentation.Four);

            Logger.Log($"Taken: {taken}, Total Needed (that match post processor): {takeCount}", Indentation.Five);

            if (taken < takeCount)
            {
                if (currentResponse != null && enumerator.MoveNext())
                {
                    taken++;

                    Logger.Log($"Previous response still has records. Taking another record. Total taken: {taken}. Total Needed: {takeCount}", Indentation.Five);

                    Current = enumerator.Current;
                    return true;
                }

                return StreamMoveNextInternal();
            }

            return false;
        }

        private bool StreamMoveNextInternal()
        {
            Logger.Log("Previous response does not have any records. Retrieving more records", Indentation.Five);

            if (recordCount != null && (recordCount == 0 || recordCount < parameters.PageSize))
            {
                Logger.Log($"Previous request only contained {recordCount} records, despite requesting {parameters.PageSize} records. No more records must exist", Indentation.Six);

                return false;
            }

            var newPageSize = Math.Min(initialPageSize, takeCount - taken);

            if (attempts >= 2)
            {
                Logger.Log($"Request has taken more than 2 requests. Increasing page size from {newPageSize} to {initialPageSize}", Indentation.Six);
                newPageSize = initialPageSize;
            }

            parameters.PageSize = newPageSize;
            parameters.Count = newPageSize;

            if (attempts > 0)
                parameters.Start = initialStart + processed;

            recordCount = 0;

            if (parameters.Start != null && parameters.Start != parameters.StartOffset && totalExist == -1)
            {
                Logger.Log("Request now has start offset. Calculating total number of records that exist", Indentation.Six);

                totalExist = getCount();

                if (processed >= totalExist)
                {
                    Logger.Log($"{totalExist} records exist, however have already processed {processed}. Aborting stream");

                    return false;
                }
                else
                {
                    Logger.Log($"{totalExist} records exist. Caching total", Indentation.Six);

                    getCount = () => totalExist;
                }
            }

            //Yield all the objects we asked for in the request. This does not generate any additional web requests,
            //since the streamer is simply yielding all of the elements contained in its list.
            currentResponse = streamer(parameters, getCount).Take(newPageSize);

            //Record whether there were even any records returned in the last request before filtering them.
            //If we didn't even return anything, we'll abort on the next iteration started at the end of this
            //else block
            currentResponse = HadRecords(currentResponse);

            //Apply the filter action
            currentResponse = postProcessor(currentResponse);
            enumerator = currentResponse.GetEnumerator();

            processed += parameters.PageSize;

            attempts++;

            return MoveNext();
        }

        private IEnumerable<TObject> HadRecords(IEnumerable<TObject> records)
        {
            Logger.Log("Checking whether response contained any records", Indentation.Five);

            foreach (var record in records)
            {
                Logger.Log($"Yielding record {record}", Indentation.Six);
                recordCount++;
                yield return record;
            }

            Logger.Log($"Response turned out to contain {recordCount} records", Indentation.Five);
        }

        private bool FixedMoveNext()
        {
            if (currentResponse == null)
            {
                currentResponse = streamer(parameters, getCount);
                currentResponse = postProcessor(currentResponse).Take(takeCount);

                if (postProcessorSorter != null)
                    currentResponse = postProcessorSorter(currentResponse);

                enumerator = currentResponse.GetEnumerator();
            }

            if (enumerator.MoveNext())
            {
                Current = enumerator.Current;
                return true;
            }

            return false;
        }

        [ExcludeFromCodeCoverage]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        public TObject Current { get; private set; }

        [ExcludeFromCodeCoverage]
        object IEnumerator.Current => Current;

        public IEnumerator<TObject> GetEnumerator() => this;

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
