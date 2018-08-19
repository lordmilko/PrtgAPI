using System.Collections.Generic;
using System.Linq.Expressions;
using PrtgAPI.Parameters;

namespace PrtgAPI.Linq.Expressions
{
    abstract class QueryHelper<TObject>
    {
        public abstract Expression FlagKeep(Expression expr);

        public abstract List<List<SearchFilter>> AdjustFilters(List<SearchFilter> filters);

        public abstract void FixupParameters(IParameters parameters);

        public abstract IEqualityComparer<TObject> GetComparer();

        public abstract bool CanLimitProperties(List<List<SearchFilter>> filterSets);
    }
}
