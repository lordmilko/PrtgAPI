using System.Collections.ObjectModel;

namespace PrtgAPI.CodeGenerator.Model
{
    /// <summary>
    /// Represents an inline method specification or a method that implements a template.
    /// </summary>
    internal interface IMethodImpl : IElementImpl
    {
        string Name { get; }

        string Type { get; }

        string Description { get; }

        string Template { get; }

        bool Stream { get; }

        bool Query { get; }

        bool Region { get; }

        bool PluralRegion { get; }

        ReadOnlyCollection<RegionDef> Regions { get; }

        ReadOnlyCollection<MethodDef> MethodDefs { get; }

        MethodType[] Types { get; }

        string GetValue(string value, bool spaces = true);
    }
}