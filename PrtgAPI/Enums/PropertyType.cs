using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    internal enum PropertyType
    {
        [Attributes.PropertyType(typeof(BasicObjectSetting))]
        BasicObjectSetting,

        [Attributes.PropertyType(typeof(ExeScriptSetting))]
        ExeScriptSetting,

        [Attributes.PropertyType(typeof(ScanningInterval))]
        ScanningInterval,

        [Attributes.PropertyType(typeof(SensorDisplay))]
        SensorDisplay,
    }
}
