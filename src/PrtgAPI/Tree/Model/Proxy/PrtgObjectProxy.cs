using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    internal interface IPrtgObjectProxy
    {
        void SetParentResolver(Func<PrtgNode> resolveParent);
    }

    [ExcludeFromCodeCoverage]
    internal abstract class PrtgObjectProxy<T> : IPrtgObjectProxy, IPrtgObject, ITreeValue where T : PrtgObject
    {
        #region PrtgObject

        int? ITreeValue.Id => Id;

        public int Id => Resolved.Id;

        public int ParentId => Resolved.ParentId;

        public string Name => Resolved.Name;

        public string[] Tags => Resolved.Tags;

        public string DisplayType => Resolved.DisplayType;

        public StringEnum<ObjectType> Type => Resolved.Type;

        public bool Active => Resolved.Active;

        #endregion

        //There are two ways you can proxy a value. By specifying a Func<T>, or by specifying a set of parameters that will construct the T for you

        int mode;

        private Func<T> valueResolver;

        private Func<int, T> parametersResolver;

        private Func<PrtgNode> parametersParentResolver;

        private T resolved;

        protected T Resolved
        {
            get
            {
                if (resolved != null)
                    return resolved;

                switch (mode)
                {
                    case 1:
                        resolved = valueResolver();

                        return resolved;
                    case 2:
                        if (parametersParentResolver == null)
                            throw new InvalidOperationException($"Cannot retrieve value; {nameof(parametersParentResolver)} was not set.");

                        resolved = parametersResolver(parametersParentResolver().Value.Id.Value);

                        return resolved;

                    default:
                        throw new NotImplementedException($"Don't know how to handle mode '{mode}'.");
                }
            }
        }

        protected PrtgObjectProxy(Func<T> valueResolver)
        {
            this.valueResolver = valueResolver;
            mode = 1;
        }

        protected PrtgObjectProxy(Func<int, T> parametersResolver)
        {
            this.parametersResolver = parametersResolver;
            mode = 2;
        }

        public void SetParentResolver(Func<PrtgNode> resolveParent)
        {
            this.parametersParentResolver = resolveParent;
        }
    }
}
