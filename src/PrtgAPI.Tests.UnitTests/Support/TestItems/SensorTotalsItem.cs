using System.Linq;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class SensorTotalsItem
    {
        public string PrtgVersion { get; set; }
        //public string Summary { get; private set; } //todo: implement. need gettreenodestats to return unique values so we know which fields which
        public string Summary => $"{UndefinedSens},{UpSens},{WarnSens},{DownSens},{DownAckSens},{UnusualSens},{PausedSens},{PartialDownSens}";
        public string UpSens { get; set; }
        public string DownSens { get; set; }
        public string WarnSens { get; set; }
        public string DownAckSens { get; set; }
        public string PartialDownSens { get; set; }
        public string UnusualSens { get; set; }
        public string PausedSens { get; set; }
        public string UndefinedSens { get; set; }
        public string TotalSens => CalculateTotals(UpSens, DownSens, WarnSens, DownAckSens, PartialDownSens, UnusualSens, PausedSens, UndefinedSens);
        public string ServerTime { get; set; }

        public SensorTotalsItem(string prtgVersion = "1.2.3.4", string upSens = "241", string downSens = "6", string warnSens = "6", string downAckSens = "1",
            string partialDownSens = "1", string unusualSens = "12", string pausedSens = "203", string undefinedSens = "9", string serverTime = "13/01/2017 9:00:17 PM")
        {
            PrtgVersion = prtgVersion;
            UpSens = upSens;
            DownSens = downSens;
            WarnSens = warnSens;
            DownAckSens = downAckSens;
            PartialDownSens = partialDownSens;
            UnusualSens = unusualSens;
            PausedSens = pausedSens;
            UndefinedSens = undefinedSens;
            ServerTime = serverTime;
        }

        private string CalculateTotals(params string[] sensors)
        {
            var total = sensors.Sum(TypeHelpers.StrToInt);

            return total.ToString();
        }
    }
}
