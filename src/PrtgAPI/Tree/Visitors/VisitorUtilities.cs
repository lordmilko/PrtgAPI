using System;
using System.Collections.Generic;
using PrtgAPI.Linq;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Provides common functionality used for visiting <see cref="TreeNode"/> objects.
    /// </summary>
    internal static class VisitorUtilities
    {
        /// <summary>
        /// Visits all elements of a list and returns either the original or a new collection if any of the elements
        /// were modified.<para/>
        /// If the result of visiting a given element is null, that element will be excluded from the resulting list.
        /// </summary>
        /// <typeparam name="T">The type of element the list contains.</typeparam>
        /// <param name="list">The list of elements to visit.</param>
        /// <param name="elementVisitor">A function that visits each element.</param>
        /// <returns></returns>
        internal static IReadOnlyList<T> VisitList<T>(INodeList<T> list, Func<T, T> elementVisitor)
        {
            var newItems = new List<T>(list.Count);
            bool modified = false;

            foreach (var item in list)
            {
                var newItem = elementVisitor(item);

                if (!Equals(newItem, item))
                {
                    modified = true;

                    if (newItem != null)
                        newItems.Add(newItem);
                }
                else
                    newItems.Add(newItem);
            }

            //If our list was modified, no point in wrapping it as INodeList<T> now; that'll happen when the list is incorporated into a TreeOrphan or TreeNode
            if (modified)
                return newItems.ToReadOnly();

            return list;
        }
    }
}
