using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Request
{
    [DebuggerDisplay("English = {EnglishValue}, Value = {Value}, ForeignNames = {ForeignNames,nq}")]
    class ForeignEnumValue
    {
        public List<string> ForeignName { get; }

        [ExcludeFromCodeCoverage]
        private string ForeignNames => string.Join(", ", ForeignName.Select(n => $"\"{n}\""));

        public string Value { get; }

        public bool Selected { get; }

        public string EnglishValue { get; }

        public ForeignEnumValue(string name, string value, bool selected, string englishValue, Type type)
        {
            if(type == typeof(NotificationAction))
               name = value.Split('|')[0] + $"|{name}";

            ForeignName = new List<string>
            {
                name
            };

            Value = value;

            //Sometimes triggers.json doesn't return the same string that was returned by table.xml; in that case, the selected item should show us which one to use
            Selected = selected;
            EnglishValue = englishValue;
        }
    }
}