namespace PrtgAPI.Parameters
{
    internal interface ISensorQueryTargetParametersProvider
    {
        ISensorQueryTargetParameters QueryParameters { get; }
    }
}