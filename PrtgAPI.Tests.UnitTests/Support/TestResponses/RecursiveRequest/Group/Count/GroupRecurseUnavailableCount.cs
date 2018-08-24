
namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class GroupRecurseUnavailableCount : GroupRecurseAvailableCount
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get the parent group "Windows Servers"
                    return base.GetResponse(address, content);
                case 2: //Get all groups under the "Windows Servers" group. Returns 3 groups
                            //Yield "Domain Controllers" from request #2. 1/9999
                case 3:     //Get all groups under the "Domain Controllers" group. Returns 3 groups
                                //Yield "Server 2003 DCs" from request #3. 2/9999
                case 4:         //Get all groups under the "Server 2003 DCs" group. Returns 2 groups
                                    //Yield "Active 2003 DCs" from request #4. 3/9999
                case 5:             //Get all groups under the "Active 2003 DCs" group. Returns 2 children, no grandchildren. 5/9999
                                    //Yield "Inactive 2003 DCs" from request #4. 6/9999
                                //Yield "Server 2008 DCs" from request #3. 7/9999
                case 6:         //Get all groups under the "Server 2008 DCs" group. 2 children, no grandchildren. 9/9999
                                //Yield "Server 2012 DCs" from request #3. 10/9999
                case 7:         //Get all groups under the "Server 2012 DCs" group. 2 children, no grandchildren. 12/9999
                            //Yield "Exchange Servers" from request #2. 13/9999
                case 8:     //Get all groups under the "Exchange Servers" group. Returns 3 groups
                                //Yield "Server 2003 Exchanges" from request #8. 14/9999
                case 9:         //Get all groups under the "Server 2003 Exchanges" group. 2 children, no grandchildren. 16/9999
                                //Yield "Server 2008 Exchanges" from request #8. 17/9999
                case 10:        //Get all groups under the "Server 2008 Exchanges" group. 2 children, no grandchildren. 19/9999
                                //Yield "Server 2012 Exchanges" from request #8. 20/9999
                case 11:        //Get all groups under the "Server 2012 Exchanges" group. 2 children, no grandchildren. 22/9999
                            //Yield "Exchange Servers" from request #2. 23/9999
                case 12:    //Get all groups under the "SQL Servers" group. Returns 3 groups
                                //Yield "Server 2003 SQLs" from request #12. 24/9999
                case 13:        //Get all groups under the "Server 2003 SQLs" group. 2 children, no grandchildren. 26/9999
                                //Yield "Server 2008 SQLs" from request #12. 27/9999
                case 14:        //Get all groups under the "Server 2008 SQLs" group. 2 children, no grandchildren. 29/9999
                                //Yield "Server 2012 SQLs" from request #12. 30/9999
                case 15:        //Get all groups under the "Server 2012 SQLs" group. 2 children, no grandchildren. 32/9999
                    return GetOffByOneBaseResponse(address, content);
                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
