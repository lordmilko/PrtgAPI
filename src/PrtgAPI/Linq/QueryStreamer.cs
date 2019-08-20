using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Linq.Expressions;
using PrtgAPI.Parameters;

namespace PrtgAPI.Linq
{
    class QueryStreamer<TObject, TParam> : IEnumerable<TObject>
        where TObject : ITableObject, IObject
        where TParam : TableParameters<TObject>
    {
        private Func<TParam, IEnumerable<TObject>> streamer;
        private List<TParam> parameterSets;
        private QueryHelper<TObject> queryHelper;

        private IEnumerable<TObject> results;

        public QueryStreamer(Func<TParam, IEnumerable<TObject>> streamer, List<TParam> parameterSets, QueryHelper<TObject> queryHelper)
        {
            this.streamer = streamer;
            this.parameterSets = parameterSets;
            this.queryHelper = queryHelper;
        }
        public IEnumerator<TObject> GetEnumerator()
        {
            //Sequentially execute all of the specified requests. If an object appears multiple times
            //in subsequent requests, we exclude it to create the illusion it was all done over one single
            //big request
            if (results == null)
            {
                if (queryHelper == null)
                {
                    results = parameterSets.SelectMany(streamer);

                    //Only include Property.Id for DistinctBy when executing multiple requests
                    if (parameterSets.Count > 1)
                        results = results.DistinctBy(o => o.Id);
                }
                else
                {
                    var sets = parameterSets.Select(p => Tuple.Create(p, streamer(p)));

                    results = DistinctByWithParameters(sets);
                }
            }

            return results.GetEnumerator();
        }

        private IEnumerable<TObject> DistinctByWithParameters(IEnumerable<Tuple<TParam, IEnumerable<TObject>>> sets)
        {
            HashSet<TParam> parameterHashSet = new HashSet<TParam>();
            HashSet<TObject> objectHashSet = new HashSet<TObject>(queryHelper.GetComparer());

            foreach (var set in sets)
            {
                parameterHashSet.Add(set.Item1);

                if (parameterHashSet.Count == 1)
                {
                    //If this is the first set, everything must be unique, like it or not!
                    foreach (var obj in set.Item2)
                    {
                        objectHashSet.Add(obj);
                        yield return obj;
                    }
                }
                else
                {
                    //If this is the second set, we're now a bit more careful about deciding what to include
                    foreach (var obj in set.Item2)
                    {
                        if (objectHashSet.Add(obj))
                            yield return obj;
                    }
                }
            }
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
