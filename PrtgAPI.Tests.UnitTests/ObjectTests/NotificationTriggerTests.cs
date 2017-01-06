using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;
using PrtgAPI.Tests.UnitTests.ObjectTests.Responses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class NotificationTriggerTests : ObjectTests<NotificationTrigger, NotificationTriggerItem, NotificationTriggerResponse>
    {
        [TestMethod]
        public void NotificationTrigger_CanDeserialize() => Object_CanDeserialize_Multiple();

        [TestMethod]
        public void NotificationAction_AllFields_HaveValues()
        {
            var objs = GetMultipleItems();

            foreach (var obj in objs)
            {
                foreach (var prop in obj.GetType().GetProperties())
                {
                    var val = prop.GetValue(obj);

                    switch (obj.Type)
                    {
                        case TriggerType.Change:
                            ChangeTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        case TriggerType.State:
                            StateTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        case TriggerType.Threshold:
                            ThresholdTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        case TriggerType.Speed:
                            SpeedTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        case TriggerType.Volume:
                            VolumeTrigger_AllFields_HaveValues(prop.Name, val);
                            break;
                        default:
                            throw new NotImplementedException($"TriggerType '{obj.Type}' does not have a property validator.");
                    }
                }
            }
        }

        private void ChangeTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Latency):
                case nameof(NotificationTrigger.Channel):
                case nameof(NotificationTrigger.Units):
                case nameof(NotificationTrigger.OffNotificationAction):
                case nameof(NotificationTrigger.EscalationLatency):
                case nameof(NotificationTrigger.EscalationNotificationAction):
                case nameof(NotificationTrigger.RepeatInterval):
                case nameof(NotificationTrigger.Threshold):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Change}' had value did not have a value."); //is threshold null or an empty string?
                    break;
            }
        }

        private void StateTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Channel):
                case nameof(NotificationTrigger.Units):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.State}' had value did not have a value.");
                    break;
            }
        }

        private void ThresholdTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Units):
                case nameof(NotificationTrigger.EscalationLatency):
                case nameof(NotificationTrigger.EscalationNotificationAction):
                case nameof(NotificationTrigger.RepeatInterval):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Threshold}' had value did not have a value.");
                    break;
            }
        }

        private void SpeedTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.EscalationLatency):
                case nameof(NotificationTrigger.EscalationNotificationAction):
                case nameof(NotificationTrigger.RepeatInterval):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Speed}' had value did not have a value.");
                    break;
            }
        }

        private void VolumeTrigger_AllFields_HaveValues(string propertyName, object val)
        {
            switch (propertyName)
            {
                case nameof(NotificationTrigger.Latency):
                case nameof(NotificationTrigger.OffNotificationAction):
                case nameof(NotificationTrigger.EscalationLatency):
                case nameof(NotificationTrigger.EscalationNotificationAction):
                case nameof(NotificationTrigger.RepeatInterval):
                    break;
                default:
                    Assert.IsTrue(val != null, $"Property '{propertyName}' of trigger type '{TriggerType.Volume}' had value did not have a value.");
                    break;
            }
        }

        protected override NotificationTriggerItem[] GetItems() => new[]
        {
            NotificationTriggerItem.ChangeTrigger(),
            NotificationTriggerItem.StateTrigger(),
            NotificationTriggerItem.ThresholdTrigger(),
            NotificationTriggerItem.SpeedTrigger(),
            NotificationTriggerItem.VolumeTrigger()
        };

        protected override List<NotificationTrigger> GetObjects(PrtgClient client) => client.GetNotificationTriggers(1234);

        protected override NotificationTriggerItem GetItem()
        {
            throw new NotSupportedException();
        }

        protected override NotificationTriggerResponse GetResponse(NotificationTriggerItem[] items) => new NotificationTriggerResponse(items);

        //Notification Trigger Parameter Tests

        [TestMethod]
        public void StateTriggerParameters_AllProperties_HaveDefault()
        {
            var state = TriggerSensorState.Down;
            var parameters = new StateTriggerParameters(1, null, ModifyAction.Add, state);

            Func<PropertyInfo, bool> additionalChecks = prop =>
            {
                var val = prop.GetValue(parameters);

                if (prop.Name == nameof(parameters.State))
                {
                    Assert.IsTrue(((Enum)val).Equals(state), $"Property '{prop.Name}' had value {val}");
                    return true;
                }

                return false;
            };

            TriggerParameters_AllProperties_HaveDefault(parameters, additionalChecks);
        }

        [TestMethod]
        public void ChangeTriggerParameters_AllProperties_HaveDefault()
        {
            var parameters = new ChangeTriggerParameters(1, null, ModifyAction.Add);

            TriggerParameters_AllProperties_HaveDefault(parameters);
        }

        [TestMethod]
        public void VolumeTriggerParameters_AllProperties_HaveDefault()
        {
            var parameters = new VolumeTriggerParameters(1, null, ModifyAction.Add);

            TriggerParameters_AllProperties_HaveDefault(parameters);
        }

        [TestMethod]
        public void SpeedTriggerParameters_AllProperties_HaveDefault()
        {
            var parameters = new SpeedTriggerParameters(1, null, ModifyAction.Add);

            TriggerParameters_AllProperties_HaveDefault(parameters);
        }

        [TestMethod]
        public void ThresholdTriggerParameters_AllProperties_HaveDefault()
        {
            var parameters = new ThresholdTriggerParameters(1, null, ModifyAction.Add);

            TriggerParameters_AllProperties_HaveDefault(parameters);
        }

        private void TriggerParameters_AllProperties_HaveDefault(TriggerParameters parameters, Func<PropertyInfo, bool> additionalChecks = null)
        {
            ReflectionHelpers.NullifyProperties(parameters);

            if (additionalChecks == null)
                additionalChecks = p => false;

            foreach (var prop in parameters.GetType().GetProperties2())
            {
                var val = prop.GetValue(parameters);

                if (prop.Name.EndsWith("NotificationAction"))
                {
                    Assert.IsTrue(val.ToString() == TriggerParameters.EmptyNotificationAction().ToString(), $"Property '{prop.Name}' had value {val}");
                }
                else
                {
                    if (!additionalChecks(prop))
                        Assert.IsTrue(val == null, $"Property '{prop.Name}' was not null");
                }
            }
        }

        [TestMethod]
        public void StateTriggerParameters_AllProperties_HaveValues()
        {
            var defaultState = TriggerSensorState.Down;

            var parameters = new StateTriggerParameters(1, null, ModifyAction.Add, defaultState)
            {
                OnNotificationAction = GetNotificationAction(),
                OffNotificationAction = GetNotificationAction(),
                EscalationNotificationAction = GetNotificationAction(),
                Latency = 60,
                EscalationLatency = 300,
                RepeatInterval = 3,
                State = TriggerSensorState.PartialDown
            };

            Func<PropertyInfo, bool> additionalChecks = prop =>
            {
                var val = prop.GetValue(parameters);

                if (prop.Name == nameof(parameters.State))
                {
                    Assert.IsTrue(!((Enum) val).Equals(defaultState), $"Property '{prop.Name}' had value {val}");
                    return true;
                }

                return false;
            };

            TriggerParameters_AllProperties_HaveValues(parameters, additionalChecks);
        }

        [TestMethod]
        public void ChangeTriggerParameters_AllProperties_HaveValues()
        {
            var parameters = new ChangeTriggerParameters(1, null, ModifyAction.Add)
            {
                OnNotificationAction = GetNotificationAction()
            };

            TriggerParameters_AllProperties_HaveValues(parameters);
        }

        [TestMethod]
        public void VolumeTriggerParameters_AllProperties_HaveValues()
        {
            var parameters = new VolumeTriggerParameters(1, null, ModifyAction.Add)
            {
                Channel = TriggerChannel.Primary,
                OnNotificationAction = GetNotificationAction(),
                Period = TriggerPeriod.Day,
                UnitSize = TriggerVolumeUnitSize.GByte
            };

            TriggerParameters_AllProperties_HaveValues(parameters);
        }

        [TestMethod]
        public void SpeedTriggerParameters_AllProperties_HaveValues()
        {
            var parameters = new SpeedTriggerParameters(1, null, ModifyAction.Add)
            {
                Channel = TriggerChannel.TrafficIn,
                Condition = TriggerCondition.Above,
                Latency = 60,
                OnNotificationAction = GetNotificationAction(),
                OffNotificationAction = GetNotificationAction(),
                Threshold = 4,
                UnitSize = TriggerUnitSize.Kbit,
                UnitTime = TriggerUnitTime.Hour
            };

            TriggerParameters_AllProperties_HaveValues(parameters);
        }

        [TestMethod]
        public void ThresholdTriggerParameters_AllProperties_HaveValues()
        {
            var parameters = new ThresholdTriggerParameters(1, null, ModifyAction.Add)
            {
                Channel = TriggerChannel.Total,
                Condition = TriggerCondition.Above,
                Latency = 30,
                OffNotificationAction = GetNotificationAction(),
                OnNotificationAction = GetNotificationAction(),
                Threshold = 3
            };

            TriggerParameters_AllProperties_HaveValues(parameters);
        }

        private void TriggerParameters_AllProperties_HaveValues(TriggerParameters parameters, Func<PropertyInfo, bool> additionalChecks = null)
        {
            if (additionalChecks == null)
                additionalChecks = p => false;

            foreach (var prop in parameters.GetType().GetProperties2())
            {
                var val = prop.GetValue(parameters);

                if (prop.Name.EndsWith("NotificationAction"))
                {
                    Assert.IsTrue(val.ToString() != TriggerParameters.EmptyNotificationAction().ToString(), $"Property '{prop.Name}' had the empty notification action");
                }
                
                else
                {
                    if(!additionalChecks(prop))
                        Assert.IsTrue(val != null, $"Property '{prop.Name}' had value did not have a value.");
                }
            }
        }

        private NotificationAction GetNotificationAction()
        {
            var webClient = new MockWebClient(new NotificationActionResponse(new[] {new NotificationActionItem()}));

            var client = new PrtgClient("prtg.example.com", "username", "12345678", AuthMode.PassHash, webClient);

            return client.GetNotificationActions().First();
        }
    }
}
