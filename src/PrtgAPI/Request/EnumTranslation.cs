using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PrtgAPI.Request
{
    [DebuggerDisplay("{Type}")]
    class EnumTranslation
    {
        internal Type Type { get; }

        internal List<ForeignEnumValue> Translations { get; }

        public EnumTranslation(Type type, List<ForeignEnumValue> translations)
        {
            Type = type;
            Translations = translations;
        }
    }
}