using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies roles PRTG Servers can take in a PRTG Cluster.
    /// </summary>
    public enum ClusterNodeType
    {
        /// <summary>
        /// Node is the Master Node. Changes made on the Master Node will be replicated to all Failover Nodes.
        /// </summary>
        [XmlEnum("clustermaster")]
        Master,

        /// <summary>
        /// Node is a Failover Node. Changes to objects cannot be made.
        /// </summary>
        [XmlEnum("failovernode")]
        Failover   
    }
}
