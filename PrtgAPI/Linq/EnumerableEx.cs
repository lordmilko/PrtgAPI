using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Linq
{
    static class EnumerableEx
    {
        public static T SingleObject<T>(this List<T> source, object value, string property = "ID") where T : IObject
        {
            if (source.Count == 1)
                return source.Single();

            if(source.Count == 0)
                throw new InvalidOperationException($"Failed to retrieve object with {property} {value}: Object does not exist");

            var str = source.Select(s => $"{s} ({s.GetId()})");

            throw new InvalidOperationException($"Failed to retrieve object with {property} {value}: Multiple objects were returned: " + string.Join(", ", str));
        }
    }
}
