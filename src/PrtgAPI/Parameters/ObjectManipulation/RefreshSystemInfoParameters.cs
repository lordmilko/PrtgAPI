namespace PrtgAPI.Parameters
{
    class RefreshSystemInfoParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.SysInfoCheckNow;

        public RefreshSystemInfoParameters(Either<Device, int> deviceOrId, SystemInfoType type) : base(deviceOrId.ToPrtgObject())
        {
            this[Parameter.Kind] = type;
        }
    }
}
