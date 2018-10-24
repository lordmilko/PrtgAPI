using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PrtgAPI.Linq
{
    /// <summary>
    /// Transforms an unordered parallel task into an <see cref="IEnumerable{T}"/> 
    /// </summary>
    /// <typeparam name="T">The type of object returned by this generator.</typeparam>
    [ExcludeFromCodeCoverage]
    class ParallelObjectGenerator<T> : IEnumerable<T>, IEnumerator<T>
    {
        private IEnumerator enumerator;

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelObjectGenerator{T}"/> class.
        /// </summary>
        public ParallelObjectGenerator(Task<Task<T>>[] tasks)
        {
            enumerator = tasks.GetEnumerator();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.<para />If this is the first time the enumerator has been advanced, the enumerator's task will be initialized and begin generating results in parallel for subsequent requests to <see cref="MoveNext"/>.
        /// </summary>
        /// <returns>True if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            var next = enumerator.MoveNext();

            try
            {
                if (next)
                    Current = ((Task<Task<T>>) enumerator.Current).Result.Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;

                throw;
            }

            return next;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}