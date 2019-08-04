using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.CodeGenerator.Model;

namespace PrtgAPI.CodeGenerator.MethodBuilder
{
    internal class MethodConfig
    {
        /// <summary>
        /// Specifies the type of request to be executed by this method (synchronous, asynchronous, stream, etc)
        /// </summary>
        public MethodType MethodType { get; }

        /// <summary>
        /// Specifies whether this method requires a <see cref="CancellationToken"/> parameter.
        /// </summary>
        public bool IsTokenInterface { get; private set; }

        /// <summary>
        /// Indicates whether this method (which does not accept a <see cref="CancellationToken"/>) has an overload that does.
        /// </summary>
        public bool HasTokenInterfaceOverload { get; private set; }

        private TokenMode tokenMode;

        /// <summary>
        /// Specifies how this method should handle formatting its <see cref="CancellationToken"/>.
        /// </summary>
        public TokenMode TokenMode
        {
            get
            {
                if (tokenMode == TokenMode.None)
                    throw new InvalidOperationException("Cannot retrieve token mode when token mode is None");

                return tokenMode;
            }
            set { tokenMode = value; }
        }

        /// <summary>
        /// Indicates whether the token parameter should be made optional via a default value.
        /// </summary>
        public bool HasDefaultToken => IsAnyToken(
            TokenMode.AutomaticDefault, TokenMode.AutomaticNamedDefault,
            TokenMode.MandatoryDefault, TokenMode.MandatoryNamedDefault
        );

        public bool RequiresTokenSummary => IsAnyToken(TokenMode.Automatic, TokenMode.AutomaticAll, TokenMode.AutomaticNamed, TokenMode.MandatoryCall, TokenMode.MandatoryNamedCall);

        /// <summary>
        /// Indicates that a cancellation token should be passed as a named parameter to a method expression.
        /// </summary>
        public bool RequiresNamedTokenArgument => IsAnyToken(TokenMode.AutomaticNamed, TokenMode.AutomaticNamedDefault, TokenMode.MandatoryNamedDefault, TokenMode.MandatoryNamedCall);

        /// <summary>
        /// Indicates that a cancellation token should be passed as a positional parameter to a method expression.
        /// </summary>
        public bool RequiresPositionalTokenArgument => IsAnyToken(TokenMode.Automatic, TokenMode.AutomaticAll, TokenMode.AutomaticDefault, TokenMode.MandatoryDefault, TokenMode.MandatoryCall);

        public IMethodImpl Method { get; }

        public MethodDef MethodDef { get; }

        public DocumentConfig DocumentConfig { get; }

        private IRegion region;

        private bool IsAnyToken(params TokenMode[] modes)
        {
            foreach(var mode in modes)
            {
                if (TokenMode == mode)
                    return true;
            }

            return false;
        }

        private bool IsAnyMethodToken(params TokenMode[] modes)
        {
            foreach (var mode in modes)
            {
                if (MethodDef.TokenMode == mode)
                    return true;
            }

            return false;
        }

        public bool MethodRequired
        {
            get
            {
                if (MethodType == MethodType.Asynchronous && !MethodDef.NeedsAsync)
                    return false;

                if (MethodType == MethodType.Stream && !MethodDef.NeedsStream)
                    return false;

                if (region != null)
                {
                    if (region.IsTokenRegion)
                    {
                        //Only generate synchronous and asynchronous methods in a token region
                        //(e.g. don't create StreamSensors(SensorParameters, CancellationToken))
                        if (!MethodTypeSupportsCancellation())
                            return false;
                    }
                }

                return true;
            }
        }

        public bool NeedsTokenInterfaceOverload
        {
            get
            {
                //Don't create an overload right now; it'll be done in its own region
                if (!IsTokenInterface && region?.HasTokenRegion == true)
                    return false;

                return HasTokenInterfaceOverload;
            }
        }

        public MethodConfig GetTokenOverloadConfig()
        {
            return new MethodConfig(Method, MethodDef, MethodType, DocumentConfig, region, true);
        }

        public MethodConfig(IMethodImpl methodImpl, MethodDef methodDef, MethodType type, DocumentConfig documentConfig, IRegion region = null) :
            this(methodImpl, methodDef, type, documentConfig, region, false)
        {
        }

        private MethodConfig(IMethodImpl methodImpl, MethodDef methodDef, MethodType type, DocumentConfig documentConfig, IRegion region, bool isTokenOverload)
        {
            Method = methodImpl;
            MethodDef = methodDef;
            MethodType = type;

            if (region != null && region.Type != MethodType.Unspecified)
                MethodType = region.Type;

            DocumentConfig = documentConfig;
            this.region = region;

            CalculateCancellationTokenMode(isTokenOverload);
        }

        private void CalculateCancellationTokenMode(bool isTokenOverload)
        {
            if (region != null)
            {
                if (region.IsTokenRegion)
                {
                    //This is THE "Parameters (Cancellation Token)" region that corresponds to the previous "Parameters" region
                    IsTokenInterface = true;
                }
                else
                {
                    if (region.HasTokenRegion && MethodTypeSupportsCancellation())
                    {
                        //This is a region such as "Parameters" that requires both its synchronous and asynchronous methods
                        //have cancellation tokens. As such, we must proceed as if we would when constructing a token
                        //method for an async method
                        HasTokenInterfaceOverload = true;
                    }
                }
            }

            if (region == null || (!region.IsTokenRegion && !region.HasTokenRegion))
            {
                if (isTokenOverload)
                    IsTokenInterface = true;
                else
                {
                    if (MethodType == MethodType.Asynchronous)
                    {
                        //All Asynchronous methods support specifying a CancellationToken. The question however is whether
                        //the token is built in to the original method or whether a specialized overload is required that
                        //implements the optional token
                        if (IsAnyMethodToken(TokenMode.AutomaticDefault, TokenMode.AutomaticNamedDefault,
                            TokenMode.MandatoryDefault, TokenMode.MandatoryNamedDefault,
                            TokenMode.Manual))
                            IsTokenInterface = true; //One method with CancellationToken token = default(CancellationToken)
                        else
                            HasTokenInterfaceOverload = true; //Two methods: one with a token, one without
                    }
                    else
                    {
                        if (MethodType == MethodType.Synchronous)
                        {
                            if (MethodDef.TokenMode == TokenMode.AutomaticAll)
                                HasTokenInterfaceOverload = true;
                            else
                            {
                                if (MethodDef.TokenMode == TokenMode.MandatoryDefault || MethodDef.TokenMode == TokenMode.MandatoryNamedDefault)
                                    IsTokenInterface = true;
                            }
                        }
                    }
                }
            }

            if (MethodDef.TokenMode != TokenMode.None)
            {
                TokenMode = MethodDef.TokenMode;
            }
            else
            {
                TokenMode = TokenMode.Automatic;
            }
        }

        private bool MethodTypeSupportsCancellation()
        {
            return MethodType == MethodType.Synchronous || MethodType == MethodType.Asynchronous;
        }

        public List<Parameter> GetApplicableParameters()
        {
            var parameters = MethodDef.Parameters.ToList();

            if (IsTokenInterface)
            {
                if (!parameters.Any(p => p.Type == "CancellationToken"))
                    parameters.Add(DocumentConfig.CommonParameters["token"]);
            }

            return parameters.Where(p =>
            {
                if (MethodType != MethodType.Stream && p.StreamOnly)
                    return false;

                if (MethodType == MethodType.Stream && p.ExcludeStream)
                    return false;

                if (p.TokenOnly && !IsTokenInterface)
                    return false;

                return true;
            }).ToList();
        }

        public string GetMethodName(string name, string genericArgs)
        {
            name = Method.GetValue(name);

            if (name.Contains("{Request}"))
            {
                switch (MethodType)
                {
                    case MethodType.Synchronous:
                        name = name.Replace("{Request}", "Get");
                        break;
                    case MethodType.Asynchronous:
                        name = name.Replace("{Request}", "Get") + "Async";
                        break;
                    case MethodType.Stream:
                        name = name.Replace("{Request}", "Stream");
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                if (MethodType == MethodType.Asynchronous)
                    name += "Async";
            }

            if (genericArgs != null)
                name += $"<{Method.GetValue(genericArgs)}>";

            return name;
        }
    }
}
