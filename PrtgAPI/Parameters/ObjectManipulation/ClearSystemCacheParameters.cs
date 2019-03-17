using System;

namespace PrtgAPI.Parameters
{
    class ClearSystemCacheParameters : BaseParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function
        {
            get
            {
                if (cacheType == SystemCacheType.General)
                    return CommandFunction.ClearCache;
                if (cacheType == SystemCacheType.GraphData)
                    return CommandFunction.RecalcCache;

                throw new NotImplementedException($"Don't know how to handle cache type '{cacheType}'.");
            }
        }

        private SystemCacheType cacheType;

        public ClearSystemCacheParameters(SystemCacheType cacheType)
        {
            this.cacheType = cacheType;
        }
    }
}
