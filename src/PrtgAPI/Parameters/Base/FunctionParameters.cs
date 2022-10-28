﻿using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    internal sealed class CommandFunctionParameters : BaseParameters, ICommandParameters, ISensorQueryTargetParametersProvider
    {
        public CommandFunction Function { get; }

        public CommandFunctionParameters(CommandFunction function)
        {
            Function = function;
        }

        public ISensorQueryTargetParameters QueryParameters { get; set; }
    }

    internal sealed class XmlFunctionParameters : BaseParameters, IXmlParameters
    {
        public XmlFunction Function { get; }

        public XmlFunctionParameters(XmlFunction function)
        {
            Function = function;
        }
    }

    [ExcludeFromCodeCoverage]
    internal sealed class JsonFunctionParameters : BaseParameters, IJsonParameters
    {
        public JsonFunction Function { get; }

        public JsonFunctionParameters(JsonFunction function)
        {
            Function = function;
        }
    }
}
