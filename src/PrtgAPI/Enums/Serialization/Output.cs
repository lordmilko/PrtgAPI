namespace PrtgAPI
{
    /// <summary>
    /// Specifies how a PRTG Response should be formatted.
    /// </summary>
    enum Output
    {
        /// <summary>
        /// XML file.
        /// </summary>
        Xml,

        /// <summary>
        /// Table in XML format.
        /// </summary>
        XmlTable,

        /// <summary>
        /// CSV table.
        /// </summary>
        CsvTable,

        /// <summary>
        /// HTML table.
        /// </summary>
        Html
    }
}
