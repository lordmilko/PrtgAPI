using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class NotificationTriggerParametersTests : NotificationTriggerBaseTests
    {
        
#region Notification Trigger Parameter Tests
    #region Mandatory Fields Cannot Be Null
        #region Add

        [TestMethod]
        [TestCategory("UnitTest")]
        public void StateTriggerParameters_Add_MandatoryFields_CannotBeNull()
        {
            TriggerParameters_MandatoryFields_CannotBeNull(new StateTriggerParameters(1));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ChangeTriggerParameters_Add_MandatoryFields_CannotBeNull()
        {
            TriggerParameters_MandatoryFields_CannotBeNull(new ChangeTriggerParameters(1));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void VolumeTriggerParameters_Add_MandatoryFields_CannotBeNull()
        {
            TriggerParameters_MandatoryFields_CannotBeNull(new VolumeTriggerParameters(1));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SpeedTriggerParameters_Add_MandatoryFields_CannotBeNull()
        {
            TriggerParameters_MandatoryFields_CannotBeNull(new SpeedTriggerParameters(1));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ThresholdTriggerParameters_Add_MandatoryFields_CannotBeNull()
        {
            TriggerParameters_MandatoryFields_CannotBeNull(new ThresholdTriggerParameters(1));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void VolumeTriggerParameters_Add_ValidatesUnitSize()
        {
            var parameters = new VolumeTriggerParameters(1);

            AssertEx.Throws<InvalidOperationException>(
                () => parameters.UnitSize = DataUnit.Mbit,
                "UnitSize 'Mbit' cannot be used with VolumeTriggerParameters. Please specify one of Byte, KByte, MByte, GByte, TByte."
            );
        }

        #endregion
        #region Edit

        [TestMethod]
        [TestCategory("UnitTest")]
        public void StateTriggerParameters_Edit_CanSetUnsetValue()
        {
            TriggerParameters_Edit_CanSetUnsetValue(new StateTriggerParameters(1, 1));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ChangeTriggerParameters_Edit_CanSetUnsetValue()
        {
            TriggerParameters_Edit_CanSetUnsetValue(new ChangeTriggerParameters(1, 1));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void VolumeTriggerParameters_Edit_CanSetUnsetValue()
        {
            TriggerParameters_Edit_CanSetUnsetValue(new VolumeTriggerParameters(1, 1));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SpeedTriggerParameters_Edit_CanSetUnsetValue()
        {
            TriggerParameters_Edit_CanSetUnsetValue(new SpeedTriggerParameters(1, 1));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ThresholdTriggerParameters_Edit_CanSetUnsetValue()
        {
            TriggerParameters_Edit_CanSetUnsetValue(new ThresholdTriggerParameters(1, 1));
        }

        private void TriggerParameters_Edit_CanSetUnsetValue(TriggerParameters parameters)
        {
            var properties = PrtgAPIHelpers.GetNormalProperties(parameters.GetType());

            foreach (var prop in properties)
            {
                prop.SetValue(parameters, null);
                if (prop.Name == nameof(TriggerProperty.OnNotificationAction) || prop.Name == nameof(TriggerProperty.OffNotificationAction) || prop.Name == nameof(TriggerProperty.EscalationNotificationAction))
                {
                    Assert.IsTrue(prop.GetValue(parameters).ToString() == TriggerParameters.EmptyNotificationAction().ToString(), $"Property '{prop.Name}' was not empty.");
                }
                else
                    Assert.IsTrue(prop.GetValue(parameters) == null, $"Property '{prop.Name}' was not null.");

                object defaultValue = null;

                if (prop.PropertyType.Name == nameof(TriggerChannel))
                    defaultValue = new TriggerChannel(1234);
                else if (prop.Name == nameof(VolumeTriggerParameters.UnitSize))
                {
                    if (parameters is VolumeTriggerParameters)
                        defaultValue = TestReflectionUtilities.GetDefaultUnderlying(typeof(DataVolumeUnit)).ToString().ToEnum<DataUnit>();
                    else
                        defaultValue = TestReflectionUtilities.GetDefaultUnderlying(prop.PropertyType);
                }
                else
                    defaultValue = TestReflectionUtilities.GetDefaultUnderlying(prop.PropertyType);

                prop.SetValue(parameters, defaultValue);
                Assert.IsTrue(prop.GetValue(parameters) != null, $"Property '{prop.Name}' was null.");
            }
        }

        //for edit mode, it IS valid to set fields to null, since that indicates we're not going to edit that field
        //we then also need to analyze the url we get and confirm it doesnt have any fields in it
        //also we need to check when we add something and then nullify it it goes away

        #endregion

        private void TriggerParameters_MandatoryFields_CannotBeNull(TriggerParameters parameters)
        {
            foreach (var prop in PrtgAPIHelpers.GetNormalProperties(parameters.GetType()))
            {
                var attr = prop.GetCustomAttribute<RequireValueAttribute>();
                var valRequired = attr?.ValueRequired ?? false;

                try
                {
                    prop.SetValue(parameters, null);

                    if (valRequired)
                        Assert.Fail($"Property '{prop.Name}' requires a value however did not generate an exception.");
                }
                catch (TargetInvocationException ex)
                {
                    var inner = ex.InnerException as InvalidOperationException;

                    if (inner == null)
                        throw;

                    if (!inner.Message.StartsWith("Trigger property"))
                        throw;

                    if (!valRequired)
                        Assert.Fail($"Property '{prop.Name}' does not require a value, however the property responded as if it does.");
                }
            }
        }

    #endregion
    #region All Properties Have Default Values

        [TestMethod]
        [TestCategory("UnitTest")]
        public void StateTriggerParameters_AllProperties_HaveDefault()
        {
            var parameters = new StateTriggerParameters(1, 1);

            TriggerParameters_AllProperties_HaveDefault(parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ChangeTriggerParameters_AllProperties_HaveDefault()
        {
            var parameters = new ChangeTriggerParameters(1);

            TriggerParameters_AllProperties_HaveDefault(parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void VolumeTriggerParameters_AllProperties_HaveDefault()
        {
            var parameters = new VolumeTriggerParameters(1, 1);

            TriggerParameters_AllProperties_HaveDefault(parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SpeedTriggerParameters_AllProperties_HaveDefault()
        {
            var parameters = new SpeedTriggerParameters(1, 1);

            TriggerParameters_AllProperties_HaveDefault(parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ThresholdTriggerParameters_AllProperties_HaveDefault()
        {
            var parameters = new ThresholdTriggerParameters(1, 1);

            TriggerParameters_AllProperties_HaveDefault(parameters);
        }

        private void TriggerParameters_AllProperties_HaveDefault(TriggerParameters parameters, Func<PropertyInfo, bool> additionalChecks = null)
        {
            TestReflectionUtilities.NullifyProperties(parameters);

            if (additionalChecks == null)
                additionalChecks = p => false;

            foreach (var prop in PrtgAPIHelpers.GetNormalProperties(parameters.GetType()))
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

    #endregion
    #region All Properties Have Values

        [TestMethod]
        [TestCategory("UnitTest")]
        public void StateTriggerParameters_AllProperties_HaveValues()
        {
            var parameters = new StateTriggerParameters(1)
            {
                OnNotificationAction = GetNotificationAction(),
                OffNotificationAction = GetNotificationAction(),
                EscalationNotificationAction = GetNotificationAction(),
                Latency = 60,
                EscalationLatency = 300,
                RepeatInterval = 3,
                State = TriggerSensorState.DownPartial
            };

            Func<PropertyInfo, bool> additionalChecks = prop =>
            {
                var val = prop.GetValue(parameters);

                if (prop.Name == nameof(parameters.State))
                {
                    Assert.IsTrue(!((Enum)val).Equals(TriggerSensorState.Down), $"Property '{prop.Name}' had value {val}");
                    return true;
                }

                return false;
            };

            TriggerParameters_AllProperties_HaveValues(parameters, additionalChecks);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ChangeTriggerParameters_AllProperties_HaveValues()
        {
            var parameters = new ChangeTriggerParameters(1)
            {
                OnNotificationAction = base.GetNotificationAction()
            };

            TriggerParameters_AllProperties_HaveValues(parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void VolumeTriggerParameters_AllProperties_HaveValues()
        {
            var parameters = new VolumeTriggerParameters(1)
            {
                Channel = TriggerChannel.Primary,
                OnNotificationAction = base.GetNotificationAction(),
                Period = TriggerPeriod.Day,
                UnitSize = DataUnit.GByte
            };

            TriggerParameters_AllProperties_HaveValues(parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SpeedTriggerParameters_AllProperties_HaveValues()
        {
            var parameters = new SpeedTriggerParameters(1)
            {
                Channel = TriggerChannel.TrafficIn,
                Condition = TriggerCondition.Above,
                Latency = 60,
                OnNotificationAction = base.GetNotificationAction(),
                OffNotificationAction = base.GetNotificationAction(),
                Threshold = 4,
                UnitSize = DataUnit.Kbit,
                UnitTime = TimeUnit.Hour
            };

            TriggerParameters_AllProperties_HaveValues(parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ThresholdTriggerParameters_AllProperties_HaveValues()
        {
            var parameters = new ThresholdTriggerParameters(1)
            {
                Channel = TriggerChannel.Total,
                Condition = TriggerCondition.Above,
                Latency = 30,
                OffNotificationAction = base.GetNotificationAction(),
                OnNotificationAction = base.GetNotificationAction(),
                Threshold = 3
            };

            TriggerParameters_AllProperties_HaveValues(parameters);
        }

        private void TriggerParameters_AllProperties_HaveValues(TriggerParameters parameters, Func<PropertyInfo, bool> additionalChecks = null)
        {
            if (additionalChecks == null)
                additionalChecks = p => false;

            foreach (var prop in PrtgAPIHelpers.GetNormalProperties(parameters.GetType()))
            {
                var val = prop.GetValue(parameters);

                if (prop.Name.EndsWith("NotificationAction"))
                {
                    Assert.IsTrue(val.ToString() != TriggerParameters.EmptyNotificationAction().ToString(), $"Property '{prop.Name}' had the empty notification action");
                }

                else
                {
                    if (!additionalChecks(prop))
                        Assert.IsTrue(val != null, $"Property '{prop.Name}' had value did not have a value.");
                }
            }
        }

        #endregion
    #region Create Parameters From Existing Trigger

        [TestMethod]
        [TestCategory("UnitTest")]
        public void StateTriggerParameters_Create_FromExistingTrigger()
        {
            var trigger = GetMultipleItems().First(t => t.Type == TriggerType.State);
            var parameters = new StateTriggerParameters(1234, trigger);

            TriggerParameters_Create_FromExistingTrigger(trigger, parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ChangeTriggerParameters_Create_FromExistingTrigger()
        {
            var trigger = GetMultipleItems().First(t => t.Type == TriggerType.Change);
            var parameters = new ChangeTriggerParameters(1234, trigger);

            TriggerParameters_Create_FromExistingTrigger(trigger, parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void VolumeTriggerParameters_Create_FromExistingTrigger()
        {
            var trigger = GetMultipleItems().First(t => t.Type == TriggerType.Volume);
            var parameters = new VolumeTriggerParameters(1234, trigger);

            TriggerParameters_Create_FromExistingTrigger(trigger, parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SpeedTriggerParameters_Create_FromExistingTrigger()
        {
            var trigger = GetMultipleItems().First(t => t.Type == TriggerType.Speed);
            var parameters = new SpeedTriggerParameters(1234, trigger);

            TriggerParameters_Create_FromExistingTrigger(trigger, parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ThresholdTriggerParameters_Create_FromExistingTrigger()
        {
            var trigger = GetMultipleItems().First(t => t.Type == TriggerType.Threshold);
            var parameters = new ThresholdTriggerParameters(1234, trigger);

            TriggerParameters_Create_FromExistingTrigger(trigger, parameters);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TriggerParameters_Create_FromNullTrigger()
        {
            AssertEx.Throws<ArgumentNullException>(() => new StateTriggerParameters(1234, null), $"Value cannot be null.{Environment.NewLine}Parameter name: sourceTrigger");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TriggerParameters_Create_FromInvalidTriggerType()
        {
            var trigger = GetMultipleItems().First(t => t.Type == TriggerType.State);
            AssertEx.Throws<ArgumentException>(
                () => new ChangeTriggerParameters(1234, trigger),
                "A NotificationTrigger of type 'State' cannot be used to initialize trigger parameters of type 'Change'"
            );
        }

        private void TriggerParameters_Create_FromExistingTrigger(NotificationTrigger trigger, TriggerParameters parameters)
        {
            //shouldnt we _actually_ be checking that the values are the same?
            //and, shouldnt we be populating _all_ properties of the trigger first?

            //then, we need to make sure we can clone a trigger into some new parameters and add them successfully
            //and THEN, we need to write some tests for that

            foreach (var paramProp in PrtgAPIHelpers.GetNormalProperties(parameters.GetType()))
            {
                bool found = false;

                foreach (var triggerProp in trigger.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if ((paramProp.Name == "TriggerInternal" && triggerProp.Name == "Trigger") ||
                        (paramProp.Name == "State" && triggerProp.Name == "StateTrigger") ||
                        (paramProp.Name == triggerProp.Name))
                    {
                        found = true;
                        Assert.IsTrue(paramProp.GetValue(parameters) != null, $"Parameter '{paramProp}' was null");
                    }
                }

                if (!found)
                    Assert.Fail($"Couldn't find notification trigger property that corresponded to parameter property '{paramProp.Name}'");
            }
        }

    #endregion
#endregion
    }
}
