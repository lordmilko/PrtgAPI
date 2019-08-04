using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.Model;
using PrtgAPI.CodeGenerator.MethodBuilder.Model;

namespace PrtgAPI.CodeGenerator.MethodBuilder.Builders
{
    internal class MethodHeaderBuilder
    {
        MethodConfig methodConfig;
        XmlHelper xmlHelper;

        public MethodHeaderBuilder(MethodConfig methodConfig)
        {
            this.methodConfig = methodConfig;
            xmlHelper = new XmlHelper(methodConfig);
        }

        public MethodHeader GetHeader()
        {
            var visibility = GetVisibility();
            var returnType = GetReturnType();
            var methodName = GetMethodName();
            var parameters = GetParameters();
            var genericConstraints = GetGenericConstraints();

            return new MethodHeader(visibility, returnType, methodName, parameters, genericConstraints);
        }

        private string GetVisibility()
        {
            return "public";
        }

        private string GetReturnType()
        {
            var returnType = methodConfig.Method.GetValue(methodConfig.MethodDef.ReturnType);

            if (string.IsNullOrWhiteSpace(returnType))
                throw xmlHelper.ThrowMissing("return type");

            if (methodConfig.MethodType == MethodType.Asynchronous)
            {
                if (returnType == "void")
                    return "async Task";

                return $"async Task<{returnType}>";
            }

            if (methodConfig.MethodType == MethodType.Stream)
                return returnType.Replace("List", "IEnumerable");

            return returnType;
        }

        private string GetMethodName()
        {
            string genericArgs = null;

            if (methodConfig.MethodDef.GenericArgs.Count > 0)
                genericArgs = string.Join(", ", methodConfig.MethodDef.GenericArgs.Select(a => a.Name));

            var methodName = methodConfig.GetMethodName(methodConfig.MethodDef.Name, genericArgs);

            return methodName;
        }

        private ReadOnlyCollection<HeaderParameter> GetParameters()
        {
            var parameters = methodConfig.GetApplicableParameters();

            var list = new List<HeaderParameter>();

            var canHaveDefaultParameters = CanHaveDefaultParameters(parameters);

            foreach(var param in parameters)
            {
                if (string.IsNullOrEmpty(param.Name))
                    throw new InvalidOperationException($"MethodDef '{methodConfig.MethodDef}' has unnamed parameters");

                if (string.IsNullOrEmpty(param.Type))
                    throw new InvalidOperationException($"Parameter '{param.Name}' of methoddef '{methodConfig.MethodDef}' doesn't have a type");

                var @default = param.Default;

                if (methodConfig.MethodType == MethodType.Stream && param.StreamDefault != null)
                    @default = param.StreamDefault;

                if (!canHaveDefaultParameters)
                    @default = null;

                var type = methodConfig.Method.GetValue(param.Type);
                var name = methodConfig.Method.GetValue(param.Name);

                var parameter = MakeParameter(parameters, type, name, @default);

                list.Add(parameter);
            }

            return list.ToReadOnlyList();
        }

        private List<string> GetGenericConstraints()
        {
            var list = new List<string>();

            foreach(var arg in methodConfig.MethodDef.GenericArgs)
            {
                if (!string.IsNullOrEmpty(arg.Constraint))
                    list.Add($"{arg.Name} : {arg.Constraint}");
            }

            return list;
        }

        private bool CanHaveDefaultParameters(List<Parameter> parameters)
        {
            if (methodConfig.IsTokenInterface || methodConfig.TokenMode == TokenMode.Manual)
            {
                bool tokenHasDefault = methodConfig.HasDefaultToken || (methodConfig.TokenMode == TokenMode.Manual && parameters.Any(p => p.Type == "CancellationToken" && p.Default != null));

                if (!tokenHasDefault && parameters.Any(p => p.Default != null))
                    return false;
            }

            return true;
        }

        private HeaderParameter MakeParameter(List<Parameter> parameters, string type, string name, string @default)
        {
            if (type == "CancellationToken" && methodConfig.HasDefaultToken)
                @default = "default(CancellationToken)";

            if (methodConfig.IsTokenInterface)
            {
                var @params = "params ";

                if (type.StartsWith(@params))
                {
                    type = type.Substring(@params.Length);

                    if (methodConfig.HasDefaultToken)
                        @default = "null";
                }
            }

            return new HeaderParameter(type, name, @default);
        }
    }
}
