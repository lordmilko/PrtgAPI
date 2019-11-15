using System;
using System.Collections;
using System.Collections.Generic;

namespace PrtgAPI.Linq
{
    internal class CachedEnumerableIterator<T> : IEnumerable<T>
    {
        private IEnumerator<T> enumerator;

        private List<T> buffer = new List<T>();

        private Exception ex;
        private bool stopped;

        internal CachedEnumerableIterator(IEnumerable<T> source)
        {
            enumerator = source.GetEnumerator();
        }

        /// <summary>
        /// Generate a compiler generated enumerator for retrieving the elements from our cached buffer.
        /// </summary>
        /// <returns>A compiler generated enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            //Each time GetEnumerator() is called we reset i to 0. This single method is basically transformed
            //into a state machine by the compiler.
            var i = 0;

            while (true)
            {
                bool hasValue = false;
                var current = default(T);

                lock (enumerator)
                {
                    if (i >= buffer.Count)
                    {
                        if (!stopped)
                        {
                            try
                            {
                                hasValue = enumerator.MoveNext();

                                if (hasValue)
                                    current = enumerator.Current;
                            }
                            catch (Exception ex)
                            {
                                stopped = true;
                                this.ex = ex;

                                enumerator.Dispose();
                            }

                            if (stopped)
                            {
                                if (ex != null)
                                    throw ex;
                                else
                                    break;
                            }

                            if (hasValue)
                                buffer.Add(current);
                        }
                    }
                    else
                        hasValue = true;
                }

                if (hasValue)
                    yield return buffer[i];
                else
                    break;

                i++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
