namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies types of caches that exist on a PRTG Network Monitor server.</para>
    /// </summary>
    public enum SystemCacheType
    {
        /// <summary>
        /// Cached data for Geo Maps and Active Directory Authentication.
        /// </summary>
        General,

        /// <summary>
        /// Cached data for displaying graph data. Note: if this cache data is cleared, PRTG will be restarted.
        /// </summary>
        GraphData
    }
}
