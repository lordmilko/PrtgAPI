using PrtgAPI.CodeGenerator.CSharp;
using PrtgAPI.CodeGenerator.Model;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator
{
    #region Xml

    /// <summary>
    /// Represents the XML of a template definition. e.g. <see cref="RegionDefXml"/>, <see cref="MethodDefXml"/>.<para/>
    /// Corresponds with types that implement <see cref="IElementDef"/>.
    /// </summary>
    public interface IElementDefXml
    {
    }

    /// <summary>
    /// Represents the XML of a template implementation. e.g. <see cref="RegionImplXml"/>, <see cref="MethodImplXml"/>.<para/>
    /// Corresponds with types that implement <see cref="IElementImpl"/>.
    /// </summary>
    public interface IElementImplXml
    {
    }

    #endregion
    #region Model

    /// <summary>
    /// Represents a template definition. e.g. <see cref="RegionDef"/>, <see cref="MethodDef"/>.<para/>
    /// Corresponds with <see cref="IElementDefXml"/>.
    /// </summary>
    internal interface IElementDef
    {
    }

    /// <summary>
    /// Represents a template implementation node. e.g. <see cref="RegionImpl"/>, <see cref="MethodImpl"/>, <see cref="InlineMethodDef"/>.<para/>
    /// Corresponds with types that implement <see cref="IElementImplXml"/>.
    /// </summary>
    interface IElementImpl
    {
    }

    #endregion

    /// <summary>
    /// Represents the final transformation of a component. e.g. <see cref="Region"/>, <see cref="Method"/>.
    /// </summary>
    internal interface IElement
    {
    }
}
