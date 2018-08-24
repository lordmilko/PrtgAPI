namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    public class RealTypeProperty
    {
        public string RealName { get; set; }
        public BaseType RealBaseType { get; set; }
    }

    public class RealTypeConstructor
    {
        public string RealName { get; set; }
        public BaseType RealBaseType { get; set; }

        public RealTypeConstructor(string name, BaseType baseType)
        {
            RealName = name;
            RealBaseType = baseType;
        }
    }

    public class RealTypeHybrid
    {
        public string RealName { get; set; }
        public BaseType RealBaseType { get; set; }

        public bool RealActive { get; set; }

        public RealTypeHybrid(string name, BaseType baseType)
        {
            RealName = name;
            RealBaseType = baseType;
        }
    }

    public class RealTypeConstructorGetOnly
    {
        public string RealName { get; }
        public BaseType RealBaseType { get; }

        public RealTypeConstructorGetOnly(string name, BaseType baseType)
        {
            RealName = name;
            RealBaseType = baseType;
        }
    }
}
