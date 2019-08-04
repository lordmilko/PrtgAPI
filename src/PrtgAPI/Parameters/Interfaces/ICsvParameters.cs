namespace PrtgAPI.Parameters
{
    interface ICsvParameters : IParameters
    {
        CsvFunction Function { get; }
    }
}
