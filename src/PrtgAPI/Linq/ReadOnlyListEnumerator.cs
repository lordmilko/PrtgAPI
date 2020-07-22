using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Linq
{
    [DebuggerStepThrough]
    struct ReadOnlyListEnumerator<T> : IEnumerator<T>
    {
        private readonly IReadOnlyList<T> list;
        private int index;

        internal ReadOnlyListEnumerator(IReadOnlyList<T> list)
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
}
