using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Cmdlets;

namespace PrtgAPI.PowerShell
{
    class NewSensorDynamicParameterContainer
    {
        private NewSensorDynamicParameterCategory[] parameterCategories;

        public RuntimeDefinedParameterDictionaryEx Parameters { get; set; }

        private Lazy<NewSensorDynamicParameterCategory> activeSensorType;

        public NewSensorDynamicParameterCategory ActiveSensorType => activeSensorType.Value;

        private NewSensor newSensorCmdlet;
        private PrtgClient client;

        public NewSensorDynamicParameterContainer(NewSensor newSensorCmdlet, PrtgClient client)
        {
            InitializeParameterCategories();

            activeSensorType = new Lazy<NewSensorDynamicParameterCategory>(() => parameterCategories.Single(c => c.HasParameterSet(newSensorCmdlet)));

            this.newSensorCmdlet = newSensorCmdlet;
            this.client = client;

            Parameters = GetDynamicParameters();
        }

        private void InitializeParameterCategories()
        {
            parameterCategories = Enum.GetValues(typeof(SensorType)).Cast<SensorType>().Select(t =>
            {
                if (t == SensorType.Factory)
                    return new NewSensorDynamicParameterCategory(
                        t,
                        NewSensorDestinationType.DestinationId,
                        () => new NewSensorFactoryDefinition(),
                        (c, b) =>
                        {
                            object s;

                            if (b.TryGetValue(nameof(NewSensorFactoryDefinition.Sensor), out s))
                                c.Sensor = (Sensor) s;
                        });
                else if (t == SensorType.WmiService)
                    return new NewSensorDynamicParameterCategory(
                        t,
                        NewSensorDestinationType.Device,
                        new AlternateParameterSet(
                            ParameterSet.Manual,
                            new AlternateSensorTargetParameter("ServiceName", nameof(WmiServiceSensorParameters.Services), typeof(string[]))
                        )
                    );

                return new NewSensorDynamicParameterCategory(t, NewSensorDestinationType.Device);
            }).ToArray();
        }

        public bool IsInvokableParameterSet => ActiveSensorType?.GetParameterSet(newSensorCmdlet).Invoke == true;

        public void InvokeBeginProcessing(Dictionary<string, object> boundParameters)
        {
            if (ActiveSensorType.GetInvoker(newSensorCmdlet) != null && IsInvokableParameterSet)
            {
                ActiveSensorType.GetInvoker(newSensorCmdlet).BindParameters(boundParameters);
                ActiveSensorType.GetInvoker(newSensorCmdlet).BeginProcessing(boundParameters);
            }
        }

        public void InvokeProcessRecord()
        {
            if (IsInvokableParameterSet)
                ActiveSensorType.GetInvoker(newSensorCmdlet)?.ProcessRecord(newSensorCmdlet.MyInvocation.BoundParameters);
        }

        public void InvokeEndProcessing()
        {
            if (IsInvokableParameterSet)
                ActiveSensorType.GetInvoker(newSensorCmdlet)?.EndProcessing();
        }

        public void InvokeStopProcessing()
        {
            if (IsInvokableParameterSet)
                ActiveSensorType.GetInvoker(newSensorCmdlet)?.StopProcessing();
        }

        private RuntimeDefinedParameterDictionaryEx GetDynamicParameters()
        {
            var dictionary = new RuntimeDefinedParameterDictionaryEx();

            foreach (var parameterCategory in parameterCategories)
            {
                parameterCategory.AddDynamicParameters(newSensorCmdlet, dictionary);
            }

            return dictionary;
        }
    }
}
