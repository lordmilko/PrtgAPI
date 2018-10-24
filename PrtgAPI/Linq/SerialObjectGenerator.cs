using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PrtgAPI.Linq
{
    /// <summary>
    /// Transforms an ordered enumeration of tasks into an <see cref="IEnumerable{T}"/> 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ExcludeFromCodeCoverage]
    class SerialObjectGenerator<T> : IEnumerable<T>, IEnumerator<T>
    {
        private IEnumerator<Task<T>> enumerator;

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialObjectGenerator{T}"/> class.
        /// </summary>
        public SerialObjectGenerator(IEnumerable<Task<T>> tasks)
        {
            enumerator = tasks.GetEnumerator();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection and executes the task within.
        /// </summary>
        /// <returns>True if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            var next = enumerator.MoveNext();

            if (next)
                Current = enumerator.Current.Result;

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