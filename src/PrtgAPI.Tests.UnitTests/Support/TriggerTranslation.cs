using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support
{
    internal class TriggerTranslation
    {
        internal NotificationTriggerItem EnglishXml { get; }

        internal NotificationTriggerItem JapaneseXml { get; }

        internal NotificationTriggerJsonItem EnglishJson { get; }

        internal NotificationTriggerJsonItem JapaneseJson { get; }

        public TriggerTranslation(NotificationTriggerItem englishXml, NotificationTriggerItem japaneseXml,
            NotificationTriggerJsonItem englishJson, NotificationTriggerJsonItem japaneseJson)
        {
            EnglishXml = englishXml;
            JapaneseXml = japaneseXml;
            EnglishJson = englishJson;
            JapaneseJson = japaneseJson;
        }
    }
}
