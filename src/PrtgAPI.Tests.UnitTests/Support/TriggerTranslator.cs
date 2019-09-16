using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support
{
    internal class TriggerTranslator
    {
        private IDictionary<string, object> EnglishJson = new Dictionary<string, object>();

        private IDictionary<string, object> EnglishXml = new Dictionary<string, object>();

        private IDictionary<string, object> JapaneseJson = new Dictionary<string, object>();

        private IDictionary<string, object> JapaneseXml = new Dictionary<string, object>();

        internal static TriggerTranslation StateTrigger(object[] properties)
        {
            var translator = new TriggerTranslator();

            return translator.BuildState(properties);
        }

        internal static TriggerTranslation VolumeTrigger(object[] properties)
        {
            var translator = new TriggerTranslator();

            return translator.BuildVolume(properties);
        }

        internal static TriggerTranslation SpeedTrigger(object[] properties)
        {
            var translator = new TriggerTranslator();

            return translator.BuildSpeed(properties);
        }

        internal static TriggerTranslation ThresholdTrigger(object[] properties)
        {
            var translator = new TriggerTranslator();

            return translator.BuildThreshold(properties);
        }

        internal static TriggerTranslation ChangeTrigger(object[] properties)
        {
            var translator = new TriggerTranslator();

            return translator.BuildChange(properties);
        }

        private TriggerTranslation BuildState(object[] properties)
        {
            Latency               (properties[0]);
            EscLatency            (properties[1]);
            Repeatival            (properties[2]);
            OffNotificationAction (properties[3]);
            EscNotificationAction (properties[4]);
            OnNotificationAction  (properties[5]);
            Nodest                (properties[6]);
            NodestOptions         (properties[7]);
            ParentId              (properties[8]);
            SubId                 (properties[9]);
            TypeName              (properties[10]);

            return GetItems("StateTrigger");
        }

        private TriggerTranslation BuildSpeed(object[] properties)
        {
            Latency               (properties[0]);
            OffNotificationAction (properties[1]);
            Channel               (properties[2]);
            ChannelOptions        (properties[3]);
            Condition             (properties[4]);
            ConditionOptions      (properties[5]);
            Threshold             (properties[6]);
            UnitSize              (properties[7]);
            UnitTime              (properties[8]);
            UnitTimeOptions       (properties[9]);
            OnNotificationAction  (properties[10]);
            ParentId              (properties[11]);
            SubId                 (properties[12]);
            TypeName              (properties[13]);

            return GetItems("SpeedTrigger");
        }

        private TriggerTranslation BuildThreshold(object[] properties)
        {
            Latency               (properties[0]);
            OffNotificationAction (properties[1]);
            Channel               (properties[2]);
            ChannelOptions        (properties[3]);
            Condition             (properties[4]);
            ConditionOptions      (properties[5]);
            Threshold             (properties[6]);
            OnNotificationAction  (properties[7]);
            ParentId              (properties[8]);
            SubId                 (properties[9]);
            TypeName              (properties[10]);

            return GetItems("ThresholdTrigger");
        }

        private TriggerTranslation BuildVolume(object[] properties)
        {
            Channel              (properties[0]);
            ChannelOptions       (properties[1]);
            Threshold            (properties[2]);
            UnitSize             (properties[3]);
            Period               (properties[4]);
            PeriodOptions        (properties[5]);
            OnNotificationAction (properties[6]);
            ParentId             (properties[7]);
            SubId                (properties[8]);
            TypeName             (properties[9]);

            return GetItems("VolumeTrigger");
        }

        private TriggerTranslation BuildChange(object[] properties)
        {
            OnNotificationAction (properties[0]);
            ParentId             (properties[1]);
            SubId                (properties[2]);
            TypeName             (properties[3]);

            return GetItems("ChangeTrigger");
        }

        private TriggerTranslation GetItems(string methodName)
        {
            var method = typeof(NotificationTriggerItem).GetMethod(methodName);

            var enXml = (NotificationTriggerItem)method.Invoke(null, GetParameters(method, EnglishXml));
            var jpXml = (NotificationTriggerItem)method.Invoke(null, GetParameters(method, JapaneseXml));

            method = typeof(NotificationTriggerJsonItem).GetMethod(methodName);

            var enJson = (NotificationTriggerJsonItem)method.Invoke(null, GetParameters(method, EnglishJson));
            var jpJson = (NotificationTriggerJsonItem)method.Invoke(null, GetParameters(method, JapaneseJson));

            return new TriggerTranslation(enXml, jpXml, enJson, jpJson);
        }

        private object[] GetParameters(MethodInfo method, IDictionary<string, object> parametersMap)
        {
            var paramNames = method.GetParameters().Select(p => p.Name).ToArray();

            var parameters = new object[paramNames.Length];

            foreach (var item in parametersMap)
            {
                var paramIndex = Array.IndexOf(paramNames, item.Key);

                if (paramIndex == -1)
                    throw new InvalidOperationException($"Parameter '{item.Key}' was not found");

                parameters[paramIndex] = item.Value;
            }

            return parameters;
        }

        private void Latency(object val) => AddJsonAndXml("latency", val);

        private void EscLatency(object val) => AddJsonAndXml("escLatency", val);

        private void Repeatival(object val) => AddJsonAndXml("repeatival", val);

        private void OffNotificationAction(object val) => AddJsonAndXml("offNotificationAction", val);

        private void EscNotificationAction(object val) => AddJsonAndXml("escNotificationAction", val);

        private void OnNotificationAction(object val) => AddJsonAndXml("onNotificationAction", val);

        private void Nodest(object val) => AddJsonAndXml("nodest", val);

        private void NodestOptions(object val) => AddJsonOption("nodestInput", val);

        private void Channel(object val) => AddJsonAndXml("channel", val);

        private void ChannelOptions(object val) => AddJsonOption("channelInput", val);

        private void Threshold(object val) => AddJsonAndXml("threshold", val);

        private void UnitSize(object val) => AddJsonAndXml("unitSize", val);

        private void UnitSizeOptions(object val) => AddJsonOption("unitSizeInput", val);

        private void Period(object val) => AddJsonAndXml("period", val);

        private void PeriodOptions(object val) => AddJsonOption("periodInput", val);

        private void Condition(object val) => AddJsonAndXml("condition", val);

        private void ConditionOptions(object val) => AddJsonOption("conditionInput", val);

        private void UnitTime(object val) => AddJsonAndXml("unitTime", val);

        private void UnitTimeOptions(object val) => AddJsonOption("unitTimeInput", val);

        private void ParentId(object val) => AddXml("parentId", val);

        private void SubId(object val) => AddJsonAndXml("subId", val);

        private void TypeName(object val) => AddJsonAndXml("typeName", val);

        private void AddJsonAndXml(string parameterName, object val)
        {
            if (!val.IsIEnumerable() || val.ToIEnumerable().ToArray().Length == 2)
            {
                AddXml(parameterName, val);
                AddJson(parameterName, val);
            }
            else
            {
                var arr = val.ToIEnumerable().ToArray();

                //There's a separate JSON and XML value. A translation will be established by analyzing what the selected option is and inspecting its value.
                //e.g. XML says "-1|None" but JSON says "-1|no notification"

                EnglishXml.Add(new KeyValuePair<string, object>(parameterName, arr[0]));
                JapaneseXml.Add(new KeyValuePair<string, object>(parameterName, arr[1]));
                EnglishJson.Add(new KeyValuePair<string, object>(parameterName, arr[2]));
                JapaneseJson.Add(new KeyValuePair<string, object>(parameterName, arr[3]));
            }
        }

        private void AddXml(string parameterName, object val)
        {
            AddDictionary(EnglishXml, JapaneseXml, parameterName, val);
        }

        private void AddJson(string parameterName, object val)
        {
            AddDictionary(EnglishJson, JapaneseJson, parameterName, val);
        }

        private void AddDictionary(IDictionary<string, object> enDict, IDictionary<string, object> jpDict,
            string parameterName, object val)
        {
            string en;
            string jp;

            if (val.IsIEnumerable())
            {
                var arr = val.ToIEnumerable().ToArray();

                en = arr[0].ToString();
                jp = arr[1].ToString();
            }
            else
            {
                en = val.ToString();
                jp = en;
            }

            enDict.Add(new KeyValuePair<string, object>(parameterName, en));
            jpDict.Add(new KeyValuePair<string, object>(parameterName, jp));
        }

        private void AddJsonOption(string parameterName, object val)
        {
            var arr = val.ToIEnumerable().ToArray();

            var en = arr.Where((v, i) => i % 2 == 0).Cast<string>().ToArray();
            var jp = arr.Where((v, i) => i % 2 != 0).Cast<string>().ToArray();

            EnglishJson.Add(new KeyValuePair<string, object>(parameterName, en));
            JapaneseJson.Add(new KeyValuePair<string, object>(parameterName, jp));
        }
    }
}
