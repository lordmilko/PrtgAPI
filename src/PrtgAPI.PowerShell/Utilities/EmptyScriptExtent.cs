using System;
using System.Management.Automation.Language;

namespace PrtgAPI.Utilities
{
    class EmptyScriptExtent : IScriptExtent
    {
        bool useDefault;

        public EmptyScriptExtent(bool useDefault)
        {
            this.useDefault = useDefault;
        }

        public string File => GetValue<string>();

        public IScriptPosition StartScriptPosition => GetValue<IScriptPosition>();

        public IScriptPosition EndScriptPosition => GetValue<IScriptPosition>();

        public int StartLineNumber => GetValue<int>();

        public int StartColumnNumber => GetValue<int>();

        public int EndLineNumber => GetValue<int>();

        public int EndColumnNumber => GetValue<int>();

        public string Text => GetValue<string>();

        public int StartOffset => GetValue<int>();

        public int EndOffset => GetValue<int>();

        private T GetValue<T>()
        {
            if (useDefault)
            {
                if (typeof(T) == typeof(string))
                    return (T) (object) string.Empty;

                return default(T);
            }

            throw new NotImplementedException();
        }
    }
}
