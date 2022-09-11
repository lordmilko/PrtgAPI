using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for creating a new Sensor Factory sensor.
    /// </summary>
    public class FactorySensorParameters : SensorParametersInternal
    {
        internal override string[] DefaultTags => new[] { "factorysensor" };

        /// <summary>
        /// Initializes a new instance of the <see cref="FactorySensorParameters"/> class.
        /// </summary>
        /// <param name="channelDefinition">The channel definition to use for the sensor.</param>
        /// <param name="sensorName">The name to use for the sensor.</param>
        /// <param name="errorMode">How the sensor should respond when a source channel is in an error state.</param>
        /// <param name="errorFormula">The custom formula to use when <paramref name="errorMode"/> is <see cref="FactoryErrorMode.CustomFormula"/>.</param>
        /// <param name="missingDataMode">The value a channel should use when one of its source channels is not reporting a value.</param>
        public FactorySensorParameters(IEnumerable<string> channelDefinition, string sensorName = "Sensor Factory",
            FactoryErrorMode errorMode = FactoryErrorMode.ErrorOnError, string errorFormula = null,
            FactoryMissingDataMode missingDataMode = FactoryMissingDataMode.DontCalculate) : base(sensorName, SensorType.Factory)
        {
            if (channelDefinition == null)
                throw new ArgumentNullException(nameof(channelDefinition));

            ChannelDefinition = channelDefinition as string[] ?? channelDefinition.ToArray();

            FactoryErrorMode = errorMode;
            FactoryErrorFormula = errorFormula;
            FactoryMissingDataMode = missingDataMode;
        }

        /// <summary>
        /// Gets or sets the channel definition to use for the factory.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(ObjectProperty.ChannelDefinition)]
        public string[] ChannelDefinition
        {
            get { return GetCustomParameterArray(ObjectProperty.ChannelDefinition, '\r', '\n'); }
            set { SetCustomParameterArray(ObjectProperty.ChannelDefinition, value, '\r', '\n'); }
        }

        /// <summary>
        /// Gets or sets how the factory should respond when one of its source sensors enters an error state.
        /// </summary>
        [PropertyParameter(ObjectProperty.FactoryErrorMode)]
        public FactoryErrorMode FactoryErrorMode
        {
            get { return (FactoryErrorMode)GetCustomParameterEnumXml<FactoryErrorMode>(ObjectProperty.FactoryErrorMode); }
            set { SetCustomParameterEnumXml(ObjectProperty.FactoryErrorMode, value); }
        }

        /// <summary>
        /// Gets or sets a custom formula to use for determining when the factory should enter an error state.
        /// </summary>
        [PropertyParameter(ObjectProperty.FactoryErrorFormula)]
        public string FactoryErrorFormula
        {
            get { return (string)GetCustomParameter(ObjectProperty.FactoryErrorFormula); }
            set { SetCustomParameter(ObjectProperty.FactoryErrorFormula, value); }
        }

        /// <summary>
        /// Gets or sets the value to use for channels when their source sensors are not reporting data.
        /// </summary>
        [PropertyParameter(ObjectProperty.FactoryMissingDataMode)]
        public FactoryMissingDataMode FactoryMissingDataMode
        {
            get { return (FactoryMissingDataMode)GetCustomParameterEnumXml<FactoryMissingDataMode>(ObjectProperty.FactoryMissingDataMode); }
            set { SetCustomParameterEnumXml(ObjectProperty.FactoryMissingDataMode, value); }
        }
    }
}
