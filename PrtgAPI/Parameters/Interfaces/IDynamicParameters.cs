namespace PrtgAPI.Parameters
{
    interface IDynamicParameters : IParameters
    {
        object this[string name] { get; set; }
    }
}
