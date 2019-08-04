namespace PrtgAPI
{
    /// <summary>
    /// Specifies the level of level of transformation and validation to perform on a serialized <see cref="SearchFilter.Value"/> based on the <see cref="SearchFilter.Property"/>'s required processing rules.
    /// </summary>
    public enum FilterMode
    {
        /// <summary>
        /// Apply all normal transformations and validations.
        /// </summary>
        Normal,

        /// <summary>
        /// Apply all normal transformations however do not perform validations.
        /// </summary>
        Illegal,

        /// <summary>
        /// Do not perform any transformations or validations.
        /// </summary>
        Raw
    }
}
