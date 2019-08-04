using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.TreeNodes;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupDeepNestingScenario : GroupScenario
    {
        public GroupDeepNestingScenario()
        {
            probe = new ProbeNode("Local Probe",
                new GroupNode("Servers",
                    new GroupNode("Windows Servers",
                        new GroupNode("Domain Controllers",
                            new GroupNode("Server 2003 DCs",
                                new GroupNode("Active 2003 DCs",
                                    new GroupNode("Fully Active 2003 DCs"),
                                    new GroupNode("Partially Active 2003 DCs")
                                ),
                                new GroupNode("Inactive 2003 DCs")
                            ),
                            new GroupNode("Server 2008 DCs",
                                new GroupNode("Active 2008 DCs"),
                                new GroupNode("Inactive 2008 DCs")
                            ),
                            new GroupNode("Server 2012 DCs",
                                new GroupNode("Active 2012 DCs"),
                                new GroupNode("Inactive 2012 DCs")
                            )
                        ),
                        new GroupNode("Exchange Servers",
                            new GroupNode("Server 2003 Exchanges",
                                new GroupNode("Active 2003 Exchanges"),
                                new GroupNode("Inactive 2003 Exchanges")
                            ),
                            new GroupNode("Server 2008 Exchanges",
                                new GroupNode("Active 2008 Exchanges"),
                                new GroupNode("Inactive 2008 Exchanges")
                            ),
                            new GroupNode("Server 2012 Exchanges",
                                new GroupNode("Active 2012 Exchanges"),
                                new GroupNode("Inactive 2012 Exchanges")
                            )
                        ),
                        new GroupNode("SQL Servers",
                            new GroupNode("Server 2003 SQLs",
                                new GroupNode("Active 2003 SQLs"),
                                new GroupNode("Inactive 2003 SQLs")
                            ),
                            new GroupNode("Server 2008 SQLs",
                                new GroupNode("Active 2008 SQLs"),
                                new GroupNode("Inactive 2008 SQLs")
                            ),
                            new GroupNode("Server 2012 SQLs",
                                new GroupNode("Active 2012 SQLs"),
                                new GroupNode("Inactive 2012 SQLs")
                            )
                        )
                    )
                ),
                new GroupNode("Clients",
                    new DeviceNode("wks-1",
                        new SensorNode("Ping")
                    )
                )
            );
        }

        protected GroupNode Servers => probe.Groups.First(g => g.Name == "Servers");

        protected List<GroupNode> WindowsServers => Servers.Groups;

        protected List<GroupNode> DomainExchangeSqlServers => Servers.Groups.SelectMany(g => g.Groups).ToList();

        #region Domain Controllers

        protected List<GroupNode> DomainControllerDCs => DomainExchangeSqlServers.First(g => g.Name == "Domain Controllers").Groups;

        protected List<GroupNode> Server2003DCs => DomainControllerDCs.First(g => g.Name == "Server 2003 DCs").Groups;

        protected List<GroupNode> Active2003DCs => Server2003DCs.First(g => g.Name == "Active 2003 DCs").Groups;

        private List<GroupNode> Server2008DCs => DomainControllerDCs.First(g => g.Name == "Server 2008 DCs").Groups;

        private List<GroupNode> Server2012DCs => DomainControllerDCs.First(g => g.Name == "Server 2012 DCs").Groups;

        #endregion
        #region Exchange Servers

        private List<GroupNode> ExchangeServerExchanges => DomainExchangeSqlServers.First(g => g.Name == "Exchange Servers").Groups;

        private List<GroupNode> Server2003Exchanges => ExchangeServerExchanges.First(g => g.Name == "Server 2003 Exchanges").Groups;

        private List<GroupNode> Server2008Exchanges => ExchangeServerExchanges.First(g => g.Name == "Server 2008 Exchanges").Groups;

        private List<GroupNode> Server2012Exchanges => ExchangeServerExchanges.First(g => g.Name == "Server 2012 Exchanges").Groups;

        #endregion
        #region SQL Servers

        private List<GroupNode> SQLServerSQLs => DomainExchangeSqlServers.First(g => g.Name == "SQL Servers").Groups;

        private List<GroupNode> Server2003SQLs => SQLServerSQLs.First(g => g.Name == "Server 2003 SQLs").Groups;

        private List<GroupNode> Server2008SQLs => SQLServerSQLs.First(g => g.Name == "Server 2008 SQLs").Groups;

        private List<GroupNode> Server2012SQLs => SQLServerSQLs.First(g => g.Name == "Server 2012 SQLs").Groups;

        #endregion

        protected override IWebResponse GetResponse(string address, Content content)
        {
            Assert.AreEqual(Content.Groups, content);

            switch (requestNum)
            {
                case 1: //Get all groups. We say there is only one group, named "Servers"
                    return new GroupResponse(Servers.GetTestItem());

                case 2: //Get all groups under the parent group that match the initial filter (returns "Windows Servers")
                    return GetChildren(WindowsServers, 2000, address);

                case 3: //Get all groups under the child group that match the initial filter (returns "Domain Controllers", "Exchange Servers" and "SQL Servers")
                    return GetChildren(DomainExchangeSqlServers, 2002, address);

                #region Domain Controllers

                case 4: //Get all groups under "Domain Controllers" (returns "Server 2003 DCs", "Server 2008 DCs" and "Server 2012 DCs"
                    return GetChildren(DomainControllerDCs, 2003, address);

                case 5: //Get all groups under "Server 2003 DCs"
                    return GetChildren(Server2003DCs, 2006, address);

                case 6: //Get all groups under "Active 2003 DCs"
                    return GetChildren(Active2003DCs, 2009, address);

                case 7: //Get all groups under "Server 2008 DCs"
                    return GetChildren(Server2008DCs, 2007, address);

                case 8: //Get all groups under "Server 2012 DCs"
                    return GetChildren(Server2012DCs, 2008, address);

                #endregion
                #region Exchange Servers

                case 9: //Get all groups under "Exchange Servers"
                    return GetChildren(ExchangeServerExchanges, 2004, address);

                case 10: //Get all groups under "Server 2003 Exchanges"
                    return GetChildren(Server2003Exchanges, 2017, address);

                case 11: //Get all groups under "Server 2008 Exchanges"
                    return GetChildren(Server2008Exchanges, 2018, address);

                case 12: //Get all groups under "Server 2012 Exchanges"
                    return GetChildren(Server2012Exchanges, 2019, address);

                #endregion
                #region SQL Servers

                case 13: //Get all groups under "SQL Servers"
                    return GetChildren(SQLServerSQLs, 2005, address);

                case 14: //Get all groups under "Server 2003 SQLs"
                    return GetChildren(Server2003SQLs, 2026, address);

                case 15: //Get all groups under "Server 2008 SQLs"
                    return GetChildren(Server2008SQLs, 2027, address);

                case 16: //Get all groups under "Server 2012 SQLs"
                    return GetChildren(Server2012SQLs, 2028, address);

                #endregion

                default:
                    throw UnknownRequest(address);
            }
        }

        private GroupResponse GetChildren(List<GroupNode> groupChildren, int parentId, string address)
        {
            Assert.AreEqual(UnitRequest.Groups($"filter_parentid={parentId}"), address);
            return new GroupResponse(groupChildren.Select(g => g.GetTestItem()).ToArray());
        }
    }
}
