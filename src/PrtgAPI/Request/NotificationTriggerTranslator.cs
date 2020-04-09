using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request
{
    /// <summary>
    /// Helper class for deserializing Notification Triggers on non-English versions of PRTG.
    /// </summary>
    class NotificationTriggerTranslator
    {
        static IReadOnlyDictionary<PropertyCache, TriggerPropertyEntry> DangerousPropertyMap;

        private static Dictionary<Type, EnumTranslation> TranslationMap = new Dictionary<Type, EnumTranslation>();

        private object lockObj = new object();

        private List<NotificationTrigger> unparsed;
        private int currentParentId;

        static NotificationTriggerTranslator()
        {
            var typeCache = typeof(NotificationTrigger).GetTypeCache();

            var typedProperties = typeCache.Properties;
            var typedFields = typeCache.Fields;
            var rawProperties = typeof(NotificationTriggerDataTrigger).GetTypeCache().Properties;

            var dictionary = new Dictionary<PropertyCache, TriggerPropertyEntry>();

            foreach (var rawProperty in rawProperties)
            {
                var propertyParameter = rawProperty.GetAttribute<PropertyParameterAttribute>();

                //This is a property that has a corresponding member in NotificationTrigger
                if (propertyParameter != null)
                {
                    var typedProperty = typedProperties.First(p => propertyParameter.Property.Equals(p.GetAttribute<PropertyParameterAttribute>()?.Property));

                    if (!IsDangerousType(typedProperty))
                        continue;

                    var jsonName = rawProperty.GetAttribute<DataMemberAttribute>().Name;
                    var typedRawField = typedFields.First(f => f.GetAttribute<DataMemberAttribute>()?.Name == jsonName);
                    var rawInput = rawProperties.First(p => p.Property.Name == $"{rawProperty.Property.Name}Input");

                    var entry = new TriggerPropertyEntry(jsonName, (TriggerProperty) propertyParameter.Property, typedProperty, typedRawField, rawProperty, rawInput);

                    dictionary.Add(typedProperty, entry);
                }
            }

            DangerousPropertyMap = new ReadOnlyDictionary<PropertyCache, TriggerPropertyEntry>(dictionary);
        }

        private static bool IsDangerousType(PropertyCache property)
        {
            var type = Nullable.GetUnderlyingType(property.Property.PropertyType) ?? property.Property.PropertyType;

            //If the type is an enum or was defined by PrtgAPI, odds are we may run into some issues when deserializing an foreign language string
            return type.IsEnum || ReflectionExtensions.IsPrtgAPIProperty(typeof(NotificationTriggerTranslator), property.Property);
        }

        internal NotificationTriggerTranslator(List<NotificationTrigger> typed, int parentId)
        {
            unparsed = typed;
            currentParentId = parentId;
        }

        internal bool TranslateTriggers(NotificationTriggerDataTrigger[] raw, out int parentId)
        {
            //Each time we're called, iterate over all of the as yet unparsed triggers to see if we can serialize all of their properties.
            //If not, add them to the unparsed list for the next iteration, which will be performed on the most commonly occurring
            //ParentId (if any items are still left). With each iteration we may get "free wins" from previous translation attempts,
            //which will reduce the total number of triggers.json API calls we have to make

            var zipped = CreateZip(raw);

            var remainingUnparsed = TranslateProperties(zipped);

            if (remainingUnparsed.Count > 0)
            {
                //Maintain the order for unit tests like NotificationTrigger_Translates_InheritedAfterNormal_SomePropertiesMissing
                unparsed = unparsed.Where(v => remainingUnparsed.Contains(v)).ToList();

                currentParentId = GetNextParentId();
                parentId = currentParentId;
                return true;
            }

            parentId = default(int);
            return false;
        }

        private List<NotificationTrigger> TranslateProperties(Tuple<NotificationTrigger, NotificationTriggerDataTrigger>[] zipped)
        {
            //Inherited triggers won't have been retrieved from the latest API request. As such,
            //we'll skip over comparing them against their raw equivalents. After processing all the uninherited
            //triggers we may get a free win and have collected all the information we need to deserialize some of these;
            //otherwise, we'll queue them up to probe their actual parent objects
            var inheritedTriggers = unparsed.Where(t => t.ParentId != currentParentId).ToList();

            var typedProperties = typeof(NotificationTrigger).GetTypeCache().Properties;

            List<NotificationTrigger> remainingUnparsed = new List<NotificationTrigger>();

            foreach (var property in typedProperties)
            {
                TriggerPropertyEntry entry;

                if (DangerousPropertyMap.TryGetValue(property, out entry))
                {
                    var unparsedObj = TranslatePropertyFromRawOrCache(property, zipped, entry);

                    var unparsedInherited = TranslatePropertyFromCache(property, inheritedTriggers, entry);

                    if (unparsedObj != null)
                        unparsedInherited.AddRange(unparsedObj);

                    if (unparsedInherited.Count > 0)
                    {
                        foreach (var value in unparsedInherited)
                        {
                            if (!remainingUnparsed.Contains(value))
                                remainingUnparsed.Add(value);
                        }
                    }
                }
            }

            return remainingUnparsed;
        }

        private List<NotificationTrigger> TranslatePropertyFromRawOrCache(PropertyCache property, Tuple<NotificationTrigger, NotificationTriggerDataTrigger>[] zipped, TriggerPropertyEntry entry)
        {
            if (zipped.Length == 0)
            {
                var typed = unparsed.Where(t => t.ParentId == currentParentId).ToList();

                return TranslatePropertyFromCache(property, typed, entry);
            }

            foreach (var pair in zipped)
            {
                //We have some foreign word for our property. What we want to do now is lookup what the translation of it would be.
                //e.g. for the State property, we want our value to say "Down". In German, "Down" = "Fehler". Fehler -> 0 -> Down

                var typedValue = (string)entry.TypedRawField.GetValue(pair.Item1);

                //This property doesn't apply to this trigger type (e.g. channel on state triggers)
                if (typedValue == null)
                    continue;

                var enumData = GetEnumData(property, entry, pair.Item2, typedValue);

                SetTriggerValue(pair.Item1, enumData, entry, typedValue);
            }

            return null;
        }

        private List<NotificationTrigger> TranslatePropertyFromCache(PropertyCache property, List<NotificationTrigger> inheritedTriggers, TriggerPropertyEntry entry)
        {
            var unparsedInherited = new List<NotificationTrigger>();

            foreach (var trigger in inheritedTriggers)
            {
                var typedValue = (string)entry.TypedRawField.GetValue(trigger);

                //This property doesn't apply to this trigger type (e.g. channel on state triggers)
                if (typedValue == null)
                    continue;

                var underlyingType = property.Type.GetCacheValue().Underlying ?? property.Type;

                EnumTranslation translation;

                lock (lockObj)
                {
                    if (TranslationMap.TryGetValue(underlyingType, out translation))
                    {
                        if (translation.Type == typeof(NotificationAction))
                        {
                            var split = typedValue.Split('|');
                            typedValue = $"{split[0]}|{split[1]}";
                        }

                        var enumData = translation.Translations.SingleOrDefault(t => t.ForeignName.Contains(typedValue));

                        if (enumData == null && underlyingType != typeof(NotificationAction))
                        {
                            Debug.WriteLine($"Translation of '{property.Property}' value '{typedValue}' is unknown. Marking trigger 'Type = {trigger.Type}, SubId = {trigger.SubId}, Inherited = {trigger.Inherited}' as unparsed");
                            unparsedInherited.Add(trigger);
                        }

                        SetTriggerValue(trigger, enumData, entry, typedValue);
                    }
                    else
                    {
                        Debug.WriteLine($"Translation of property '{property.Property}' did not exist in cache. Cannot analyze value '{typedValue}'. Marking trigger 'Type = {trigger.Type}, SubId = {trigger.SubId}, Inherited = {trigger.Inherited}' as unparsed");
                        unparsedInherited.Add(trigger);
                    }
                }
            }

            return unparsedInherited;
        }

        private void SetTriggerValue(NotificationTrigger trigger, ForeignEnumValue enumData, TriggerPropertyEntry entry, string typedValue)
        {
            if (enumData != null && enumData.EnglishValue != null && typedValue != enumData.EnglishValue)
            {
                //Channels only really refer to StandardTriggerChannel values applied to non-sensor objects. A Disk IO sensor could
                //have a channel with ID 0 which refers to "Avg Bytes Per Read". As such, instead of updating the value right now,
                //we'll defer to trigger.SetEnumChannel(). SetEnumChannel() won't even be called if the channel name refers to a real
                //channel, so if SetEnumChannel() is called we can safely assume we'll need to use our translated value as our enum value
                if (entry.TypedProperty.Type == typeof(TriggerChannel))
                    trigger.translatedChannelName = enumData.EnglishValue;
                else
                    entry.TypedRawField.SetValue(trigger, enumData.EnglishValue);
            }
        }

        private Tuple<NotificationTrigger, NotificationTriggerDataTrigger>[] CreateZip(NotificationTriggerDataTrigger[] raw)
        {
            var unparsedSorted = unparsed.Where(t => t.ParentId == currentParentId).OrderBy(t => t.SubId).ToArray();

            if (raw != null)
            {
                var rawSorted = raw.Where(r => unparsedSorted.Any(u => u.SubId == r.SubId)).OrderBy(r => r.SubId);

                return unparsedSorted.Zip(rawSorted, Tuple.Create).ToArray();
            }
            else
                return Enumerable.Empty<Tuple<NotificationTrigger, NotificationTriggerDataTrigger>>().ToArray();
        }

        private int GetNextParentId()
        {
            return unparsed.GroupBy(t => t.ParentId).OrderByDescending(g => g.Count()).First().Key;
        }

        private ForeignEnumValue GetEnumData(PropertyCache property, TriggerPropertyEntry entry, NotificationTriggerDataTrigger raw, string typedValue)
        {
            EnumTranslation translation;

            var underlyingType = property.Type.GetCacheValue().Underlying ?? property.Type;

            ForeignEnumValue enumData;
            bool isNew = false;

            lock (lockObj)
            {
                if (!TranslationMap.TryGetValue(underlyingType, out translation))
                {
                    AddForeignEnum(underlyingType, raw, entry, out translation);
                    isNew = true;
                }

                if (translation == null)
                    return null;

                if (translation.Type == typeof(NotificationAction))
                {
                    var split = typedValue.Split('|');
                    typedValue = $"{split[0]}|{split[1]}";
                }

                //In the case of NotificationActions, we only add a translation when it's the "None" action,
                //so we aren't guaranteed to have a translation
                enumData = translation.Translations.SingleOrDefault(t => t.ForeignName.Contains(typedValue));

                if (enumData == null && !isNew)
                {
                    MaybeAddNewTranslations(underlyingType, raw, entry, translation);

                    enumData = translation.Translations.SingleOrDefault(t => t.ForeignName.Contains(typedValue));
                }

                if (enumData == null)
                    enumData = GetEnumDataFromSelectedValue(translation, entry, raw, typedValue);
            }

            return enumData;
        }

        private ForeignEnumValue GetEnumDataFromSelectedValue(EnumTranslation translation, TriggerPropertyEntry entry, NotificationTriggerDataTrigger raw, string typedValue)
        {
            //Maybe the typed value has a different value to its raw counterpart. e.g. in German,
            //the typed value might be "Min." while the german value is "Minute"

            ForeignEnumValue enumData = null;

            var rawValue = (string)entry.RawProperty.GetValue(raw);

            if (typedValue != rawValue)
            {
                var rawInput = (string)entry.RawInput.GetValue(raw);

                //e.g. condition "change" on change triggers (which thankfully is always in English) doesn't have rawInput
                if (rawInput != null)
                {
                    var selected = HtmlParser.Default.GetDropDownList(rawInput)
                        .SelectMany(d => d.Options.Where(o => o.Selected)).First();

                    enumData = translation.Translations.Single(t => t.Value == selected.Value);
                    enumData.ForeignName.Add(typedValue);
                }
            }

            return enumData;
        }

        private void AddForeignEnum(Type type, NotificationTriggerDataTrigger raw, TriggerPropertyEntry entry, out EnumTranslation translation)
        {
            //We don't have an entry

            var html = (string) entry.RawInput.GetValue(raw);

            //Special case TriggerCondition in case the first trigger retrieved with a Condition property is a Change Trigger
            if (html == null && type != typeof(TriggerCondition))
            {
                translation = null;
                return;
            }

            List<ForeignEnumValue> values = new List<ForeignEnumValue>();

            if (html != null)
            {
                values.AddRange(HtmlParser.Default.GetDropDownList(html).SelectMany(
                    d => d.Options.Select(o => new ForeignEnumValue(o.InnerHtml, o.Value, o.Selected, GetEnglishValue(entry.Property, type, o.Value), type))
                ).DistinctBy(v => v.Value));
            }

            //TriggerCondition.Change is a special case that's not listed in the HTML DropDown list options. It never
            //needs translating, so we set its English value to null.
            if (type == typeof(TriggerCondition))
            {
                values.Add(new ForeignEnumValue("change", "change", false, null, type));
            }

            translation = new EnumTranslation(type, values);

            TranslationMap[type] = translation;
        }

        private void MaybeAddNewTranslations(Type type, NotificationTriggerDataTrigger raw, TriggerPropertyEntry entry, EnumTranslation translation)
        {
            //We have an entry, but maybe it doesn't contain our language

            var html = (string) entry.RawInput.GetValue(raw);

            if (html == null)
                return;

            var values = HtmlParser.Default.GetDropDownList(html).SelectMany(
                d => d.Options.Select(o => new ForeignEnumValue(o.InnerHtml, o.Value, o.Selected, GetEnglishValue(entry.Property, type, o.Value), type))
            ).ToList();

            foreach (var value in values)
            {
                var match = translation.Translations.FirstOrDefault(t => t.Value == value.Value);

                if (match == null)
                    translation.Translations.Add(value);
                else
                {
                    var newNames = value.ForeignName.Where(n => !match.ForeignName.Contains(n)).ToArray();

                    if (newNames.Length > 0)
                        match.ForeignName.AddRange(newNames);
                }
            }
        }

        private string GetEnglishValue(TriggerProperty property, Type type, string value)
        {
            switch (property)
            {
                case TriggerProperty.State:
                case TriggerProperty.UnitSize:
                case TriggerProperty.UnitTime:
                case TriggerProperty.Period:
                case TriggerProperty.Condition:
                    return ToXmlEnum(type, value);

                case TriggerProperty.OnNotificationAction:
                case TriggerProperty.OffNotificationAction:
                case TriggerProperty.EscalationNotificationAction:
                    return ToAction(value);

                case TriggerProperty.Channel:
                    return ToChannel(value);

                default:
                    throw new NotImplementedException($"Don't know how to handle property '{property}'.");
            }
        }

        private string ToXmlEnum(Type type, string value)
        {
            //Verify we can convert the specified value to an enum
            var e = (Enum) EnumExtensions.XmlToEnum(value, type, typeof(XmlEnumAttribute), allowParse: false);

            if (e == null)
                throw new InvalidOperationException($"Could not find an {nameof(XmlEnumAttribute)} on type '{type.FullName}' containing value '{value}'. This should not be possible.");
            
            return e.ToString();
        }

        private string ToAction(string value)
        {
            if (value?.StartsWith("-1|") == true)
                return ((ISerializable) TriggerParameters.EmptyNotificationAction()).GetSerializedFormat();

            return null;
        }

        private string ToChannel(string value)
        {
            var v = EnumExtensions.XmlToEnum<XmlEnumAttribute>(value, typeof(StandardTriggerChannel), allowParse: false);

            return v?.ToString();
        }
    }
}
