using System.ComponentModel;

namespace PrtgAPI
{
    enum JsonFunction
    {
        [Description("getpasshash.htm")]
        GetPassHash,

        [Description("triggers.json")]
        Triggers,

        [Description("geolocator.htm")]
        GeoLocator,

        [Description("sensortypes.json")]
        SensorTypes,


        [Description("getstatus.htm")]
        GetStatus
    }
}
