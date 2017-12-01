using System;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tests.IntegrationTests.ActionTests.Types
{
    public class ExeXmlRawSensorParameters : RawSensorParameters
    {
        public ExeXmlRawSensorParameters(string sensorName, string sensorType, string exeName) : base(sensorName, sensorType)
        {
            ExeName = exeName;

            Tags = new[] {"xmlexesensor"};
            Priority = Priority.Three;
            ExeParameters = null;
            SetExeEnvironmentVariables = false;
            UseWindowsAuthentication = false;
            Mutex = null;
            Timeout = 60;
            DebugMode = DebugMode.Discard;
            InheritInterval = true;
            Interval = ScanningInterval.SixtySeconds;
            IntervalErrorMode = IntervalErrorMode.OneWarningThenDown;
        }

        public new string[] Tags
        {
            get { return GetCustomParameterArray("tags_", ' '); }
            set { SetCustomParameterArray("tags_", value, ' '); }
        }

        public Priority Priority
        {
            get { return (Priority)GetCustomParameterEnumXml<Priority>("priority_"); }
            set { SetCustomParameterEnumXml("priority_", value); }
        }

        [RequireValue(true)]
        public string ExeName
        {
            get { return ((ScriptName)GetCustomParameterInternal("exefile_")).Name; }
            set { SetCustomParameterInternal("exefile_", new ScriptName(value)); }
        }

        public string ExeParameters
        {
            get { return (string)GetCustomParameterInternal("exeparams_"); }
            set { SetCustomParameterInternal("exeparams_", value); }
        }

        public bool? SetExeEnvironmentVariables
        {
            get { return GetCustomParameterBool("environment_"); }
            set { SetCustomParameterBool("environment_", value); }
        }

        public bool? UseWindowsAuthentication
        {
            get { return GetCustomParameterBool("usewindowsauthentication_"); }
            set { SetCustomParameterBool("usewindowsauthentication_", value); }
        }

        public string Mutex
        {
            get { return (string)GetCustomParameterInternal("mutexname_"); }
            set { SetCustomParameterInternal("mutexname_", value); }
        }

        public int Timeout
        {
            get { return (int)GetCustomParameterInternal("timeout_"); }
            set
            {
                if (value > 900)
                    throw new ArgumentException("Timeout must be less than 900");

                if (value < 1)
                    throw new ArgumentException("Timeout must be greater than or equal to 1");

                SetCustomParameterInternal("timeout_", value);
            }
        }

        public DebugMode DebugMode
        {
            get { return (DebugMode)GetCustomParameterEnumXml<DebugMode>("writeresult_"); }
            set { SetCustomParameterEnumXml("writeresult_", value); }
        }

        public bool? InheritInterval
        {
            get { return GetCustomParameterBool("intervalgroup"); }
            set { SetCustomParameterBool("intervalgroup", value); }
        }

        public ScanningInterval Interval
        {
            get { return (ScanningInterval)GetCustomParameterInternal("interval_"); }
            set { SetCustomParameterInternal("interval_", value); }
        }

        public IntervalErrorMode IntervalErrorMode
        {
            get { return (IntervalErrorMode)GetCustomParameterEnumXml<IntervalErrorMode>("errorintervalsdown_"); }
            set { SetCustomParameterEnumXml("errorintervalsdown_", value); }
        }
    }

    internal class ScriptName
    {
        public string Name { get; set; }

        internal ScriptName(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name}|{Name}||";
        }
    }
}
