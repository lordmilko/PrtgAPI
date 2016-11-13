namespace PrtgAPI
{
    /// <summary>
    /// Specifies the type of content to retrieve from a PRTG API Request.
    /// </summary>
    public enum Content
    {
        //SensorTree,

        /// <summary>
        /// Groups used to organize devices.
        /// </summary>
        Groups,

        /// <summary>
        /// Computers and pieces of equipment that contain sensors monitored by PRTG.
        /// </summary>
        Devices,

        /// <summary>
        /// Objects that monitor and collect data - the fundamental components of PRTG.
        /// </summary>
        Sensors,

        Channels,
        /*Tickets,
        TicketData,
        Messages,
        Values,
        
        Reports,
        TopLists,*/

        /// <summary>
        /// All content types, including those unsupported by PRTG. Note: PRTG does not recognize "objects" as a valid content type, and as such this property should be used with caution.
        /// </summary>
        Objects
    }
}
