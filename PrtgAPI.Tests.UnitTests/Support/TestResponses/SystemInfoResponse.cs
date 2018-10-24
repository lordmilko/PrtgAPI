using System;
using System.Linq;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class SystemInfoResponse : IWebResponse
    {
        private SystemInfoItem[] items;

        public SystemInfoResponse(params SystemInfoItem[] items)
        {
            this.items = items ?? new SystemInfoItem[] { };
        }

        public string GetResponseText(ref string address)
        {
            var function = MultiTypeResponse.GetFunction(address);

            switch(function)
            {
                case nameof(JsonFunction.TableData):
                    return GetSystemInfo(address);
                default:
                    throw new NotImplementedException();
            }
        }

        private string GetSystemInfo(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            switch(components["category"].DescriptionToEnum<SystemInfoType>())
            {
                case SystemInfoType.System:
                    return GetResponse(SystemInfoType.System);
                case SystemInfoType.Hardware:
                    return GetResponse(SystemInfoType.Hardware);
                case SystemInfoType.Software:
                    return GetResponse(SystemInfoType.Software);
                case SystemInfoType.Processes:
                    return GetResponse(SystemInfoType.Processes);
                case SystemInfoType.Services:
                    return GetResponse(SystemInfoType.Services);
                case SystemInfoType.Users:
                    return GetResponse(SystemInfoType.Users);
                default:
                    throw new NotImplementedException();
            }
        }

        private string GetResponse(SystemInfoType type)
        {
            var itemsStr = string.Join(",", items.Where(i => i.Type == type).Select(i => i.Content));

            var str = "{\"prtg-version\":\"17.4.33.3283\",\"treesize\":6,\"sysinfo\":[" + itemsStr + "]}";

            return str;
        }
    }
}
