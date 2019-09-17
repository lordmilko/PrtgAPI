namespace PrtgAPI.Parameters.Helpers
{
    enum ConversionState
    {
        CorrectType = 0,
        Serializable = 1,
        Enumerable = 2,
        Nullable = 3,
        ImplicitConversion = 4,
        NonNullable = 5,
        ValueConversion = 6,
        Completed = 7,

        //We don't want to be able to MoveNext into special conditions
        ValueConversionWithNullCheck = 8
    }
}
