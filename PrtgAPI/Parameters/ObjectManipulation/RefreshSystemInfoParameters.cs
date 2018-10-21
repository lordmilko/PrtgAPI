namespace PrtgAPI.Parameters
{
    class RefreshSystemInfoParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.SysInfoCheckNow;

        public RefreshSystemInfoParameters(int deviceId, SystemInfoType type) : base(deviceId)
        {
            this[Parameter.Kind] = type;
        }
    }
}
