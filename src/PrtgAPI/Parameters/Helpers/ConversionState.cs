namespace PrtgAPI.Parameters.Helpers
{
    enum ConversionState
    {
        CorrectType = 0,
        Serializable = 1,
        Enumerable = 2,
        Nullable = 3,
        NonNullable = 4,
        ValueConversion = 5,
        Completed = 6,

        //We don't want to be able to MoveNext into special conditions
        ValueConversionWithNullCheck = 7
    }
}
