using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell
{
    class PSRawSensorParameters : RawSensorParameters
    {
        public PSRawSensorParameters(string sensorName, string sensorType) : base(sensorName, sensorType)
        {
        }

        [Hidden]
        public new List<CustomParameter> Parameters
        {
            get { return base.Parameters; }
            set { base.Parameters = value; }
        }
    }
}
