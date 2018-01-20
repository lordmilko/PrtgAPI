using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether PRTG should use transactional processing when executing a database query.
    /// </summary>
    public enum SqlTransactionMode
    {
        /// <summary>
        /// Don't use database transactions.
        /// </summary>
        [XmlEnum("noTransaction")]
        None,

        /// <summary>
        /// Use transactions always rollback all changes to the database.
        /// </summary>
        [XmlEnum("Rollback")]
        Rollback,

        /// <summary>
        /// Use transactions and commit any changes if all steps execute successfully.
        /// </summary>
        [XmlEnum("Commit")]
        CommitOnSuccess
    }
}
