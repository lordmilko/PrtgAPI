namespace PrtgAPI.Parameters
{
    class RestartProbeParameters : Parameters
    {
        public RestartProbeParameters(int? objectId)
        {
            if (objectId != null)
                this[Parameter.Id] = objectId;
        }
    }
}
