using System;
using PrtgAPI.Reflection.Cache;

namespace PrtgAPI.Parameters.Helpers
{
    interface IPropertyCacheResolver
    {
        PropertyCache GetPropertyCache(Enum property);
    }
}
