using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using PrtgAPI.Parameters;

namespace PrtgAPI.Linq
{
    class InfiniteLogGenerator : IEnumerator<Log>, IEnumerable<Log>
    {
        private Func<LogParameters, IEnumerable<Log>> getLogs;
        private LogParameters parameters;
        private Func<int, bool> progressCallback;
        private int interval;
        private CancellationToken token;

        private IEnumerator<Log> currentEnumerator;
        private int totalRetrieved;

        private HashSet<Log> hashSet = new HashSet<Log>(new LogEqualityComparer());

        public InfiniteLogGenerator(
            Func<LogParameters, CancellationToken, IEnumerable<Log>> getLogs,
            LogParameters parameters,
            int interval,
            Func<int, bool> progressCallback,
            CancellationToken token,
            Func<IEnumerable<Log>, IEnumerable<Log>> postProcessor = null
            )
        {
            if (postProcessor == null)
                postProcessor = l => l;

            this.getLogs = p => postProcessor(YieldRecords(getLogs(p, token))).Reverse();
            this.parameters = parameters;

            if (progressCallback == null)
                progressCallback = c => true;

            this.interval = interval;
            this.progressCallback = progressCallback;
            this.token = token;
        }

        public InfiniteLogGenerator(
            Func<LogParameters, CancellationToken, IEnumerable<Log>> getLogs,
            int? objectId, LogStatus[] status,
            int interval,
            Func<int, bool> progressCallback,
            CancellationToken token,
            Func<IEnumerable<Log>, IEnumerable<Log>> postProcessor = null
        ) : this(getLogs, new LogParameters(objectId, null, DateTime.Now, null, status), interval, progressCallback, token, postProcessor)
        {
        }

        public void Dispose()
        {
            Current = null;
        }

        public bool MoveNext()
        {
            //Is this our first request?
            if (currentEnumerator == null)
            {
                token.ThrowIfCancellationRequested();

                if (!progressCallback(totalRetrieved))
                    return false;

                currentEnumerator = getLogs(parameters).GetEnumerator();
            }

            token.ThrowIfCancellationRequested();

            while (!currentEnumerator.MoveNext())
            {
                if (!progressCallback(totalRetrieved))
                    return false;

                token.WaitHandle.WaitOne(interval * 1000);

                token.ThrowIfCancellationRequested();

                if (Current != null)
                    parameters.EndDate = Current.DateTime;

                currentEnumerator = getLogs(parameters).GetEnumerator();
            }

            token.ThrowIfCancellationRequested();

            Current = currentEnumerator.Current;

            return true;
        }

        private IEnumerable<Log> YieldRecords(IEnumerable<Log> logs)
        {
            foreach (var log in logs)
            {
                if (hashSet.Add(log))
                {
                    totalRetrieved++;
                    yield return log;
                }
            }
        }

        [ExcludeFromCodeCoverage]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        public Log Current { get; private set; }

        object IEnumerator.Current => Current;

        public IEnumerator<Log> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
