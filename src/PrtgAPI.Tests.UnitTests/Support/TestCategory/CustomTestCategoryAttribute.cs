using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.Support
{
    public class CustomTestCategoryAttribute : TestCategoryBaseAttribute
    {
        protected CustomTestCategoryAttribute(string primary, params string[] secondary) : this(Merge(primary, secondary))
        {
        }

        protected CustomTestCategoryAttribute(params string[] category)
        {
            if (category == null || category.Length == 0)
                throw new ArgumentException("At least one category must be specified.", nameof(category));

            TestCategories = category.ToList();
        }

        public override IList<string> TestCategories { get; }

        private static string[] Merge(string primary, string[] secondary)
        {
            var list = new List<string>();

            list.Add(primary);
            list.AddRange(secondary);

            return list.ToArray();
        }
    }
}
