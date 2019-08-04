using System.Collections.Generic;
using System.Xml;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator
{
    /// <summary>
    /// Represents an abstract model of a method defined in a template.
    /// </summary>
    internal interface IMethodDef
    {
        string Name { get; }

        string ReturnType { get; }

        XmlElement Summary { get; }

        XmlElement ReturnDescription { get; }

        IException Exception { get; }

        IList<IGenericArg> GenericArgs { get; }

        IList<IParameter> Parameters { get; }

        XmlElement BodyElement { get; }

        XmlElement SyncBodyElement { get; }

        XmlElement AsyncBodyElement { get; }

        XmlElement SyncAsyncBodyElement { get; }

        XmlElement StreamBodyElement { get; }

        XmlElement TokenBodyElement { get; }

        TokenMode TokenMode { get; }

        string Overload { get; }

        bool NeedsStream { get; }

        bool NeedsAsync { get; }

        MethodDefXml GetMethodDefXml();
    }
}
