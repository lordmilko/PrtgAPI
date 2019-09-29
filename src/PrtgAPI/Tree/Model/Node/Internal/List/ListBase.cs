using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Provides a common implementation for list enumeration and indexation.
    /// </summary>
    /// <typeparam name="T">The type of item that is stored in the list.</typeparam>
    internal abstract class ListBase<T> : INodeList<T>
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="ListBase{T}"/>.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ListBase{T}"/>.
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        public abstract T this[int index] { get; }

        #region Add

        public INodeList<T> Add(T node)
        {
            return Insert(Count, node);
        }

        public INodeList<T> AddRange(IEnumerable<T> nodes)
        {
            return InsertRange(Count, nodes);
        }

        #endregion
        #region Insert

        public INodeList<T> Insert(int index, T node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return InsertRange(index, new[] { node });
        }

        public INodeList<T> InsertRange(int index, IEnumerable<T> nodes)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            var list = this.ToList();
            list.InsertRange(index, nodes);

            if (list.Count == 0)
            {
                //The enumeration was empty, meaning no changes were made. To preserve our identity,
                //return ourselves
                return this;
            }
            else
                return CreateList(list);
        }

        #endregion
        #region Remove

        public INodeList<T> RemoveAt(int index)
        {
            return Remove(this[index]);
        }

        public INodeList<T> Remove(T node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return CreateList(this.Where(n => !n.Equals(node)));
        }

        #endregion
        #region Replace

        public INodeList<T> Replace(T oldNode, T newNode)
        {
            return ReplaceRange(oldNode, new[] { newNode });
        }

        public INodeList<T> ReplaceRange(T oldNode, IEnumerable<T> newNodes)
        {
            if (oldNode == null)
                throw new ArgumentNullException(nameof(oldNode));

            if (newNodes == null)
                throw new ArgumentNullException(nameof(newNodes));

            var index = IndexOf(oldNode);

            var list = this.ToList();
            list.RemoveAt(index);
            list.InsertRange(index, newNodes);

            return CreateList(list);
        }

        #endregion

        protected abstract ListBase<T> CreateList(IEnumerable<T> nodes);

        public int IndexOf(T node)
        {
            var index = 0;

            foreach (var elm in this)
            {
                if (Equals(elm, node))
                    return index;

                index++;
            }

            return -1;
        }
        #region Enumerator

        [DebuggerStepThrough]
        private struct Enumerator : IEnumerator<T>
        {
            private readonly ListBase<T> list;
            private int index;

            internal Enumerator(ListBase<T> list)
            {
                this.list = list;
                index = -1;
            }

            public bool MoveNext()
            {
                var newIndex = index + 1;

                if (newIndex < list.Count)
                {
                    index = newIndex;
                    return true;
                }

                return false;
            }

            public T Current => list[index];

            [ExcludeFromCodeCoverage]
            public void Reset()
            {
                index = -1;
            }

            [ExcludeFromCodeCoverage]
            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        #endregion
    }
}
