using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how PRTG should process the data returned from the SQL query.<para/>
    /// Note: PrtgAPI does not natively support data processing mode "Process data table" and its associated settings.
    /// </summary>
    public enum SqlProcessingMode
    {
        /// <summary>
        /// Report how long execution of the query took and how many rows were affected.
        /// </summary>
        [XmlEnum("ChangeDB")]
        Execute,

        /// <summary>
        /// Report how many rows were returned from the query.
        /// </summary>
        [XmlEnum("CountRows")]
        CountRows
    }
}
