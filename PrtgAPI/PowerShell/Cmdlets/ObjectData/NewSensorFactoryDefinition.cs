using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Reflection;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a channel definition for use in a PRTG Sensor Factory.</para>
    /// 
    /// <para type="description">The New-SensorFactoryDefinition cmdlet automatically defines a series of channel defitions for use in a Sensor Factory sensor.</para>
    /// 
    /// <para type="description">New-SensorFactoryDefinition can be used to create both individual channel definitions for a collection of sensors, as well as
    /// aggregation sensors, using a complex formula that operates on all of the sensors in a set.</para>
    /// 
    /// <para type="description">When specifying an Expression, the default expression and current sensor can be accessed via the $expr and $_ automatic variables.
    /// Unless you wish to modify the sensor ID or channel ID to be used for a specific sensor, it is generally recommended to avoid recalculating
    /// the base channel definition and use the automatic $expr variable, which is defined as "channel(sensorId, channelID)" where sensorID is the ID of the current
    /// sensor, and channelID is the value passed to -ChannelID.</para>
    /// 
    /// <para type="description">When specifying an Aggregator, the running accumulator, default expression and current sensor can be accessed via the $acc, $expr and $_
    /// automatic variables respectively. Based on whether not the -Expression parameter is specified, $expr will either contain the custom or the
    /// default expression.</para>
    /// 
    /// <para type="description">While it is possible to override the expression evaluated in $expr in the Aggregator by recalculating the channel definition
    /// (via the $_ automatic variable), if the channel ID specified in the new definition is different from the channel ID specified in the -ChannelID parameter,
    /// the first channel definition in the resulting output will have a different channel ID than all the rest. This is due to the fact when the Aggregator runs,
    /// it initially sets the accumulator ($acc) to the expression of the first sensor. As such, if in the Aggregator you change the channel ID, you
    /// will also need to replace the channel ID of the initial value in the accumulator. Due to the complexity involved in managing this, it is
    /// recommended to avoid modifying the channel ID in the aggregator, and either set the channel ID in an -Expression or in the -ChannelID.</para>
    /// 
    /// <para type="description">Both -Expression and -Aggregator support the use of Sensor Factory formula functions (channel(), min(), max(), avg() and percent()),
    /// as well as all boolean and math operators. For more information, see the PRTG Manual Sensor Factory documentation.</para>
    /// 
    /// <para type="description">Generating horizontal lines is not supported. To include horizontal lines in your sensor factory you must create the horizontal
    /// line channel definition manually. It does not matter what the ID of any horizontal lines are, as long as they do not conflict with any other channel definitions.</para> 
    /// 
    /// <para type="description">To automatically copy the output of New-SensorFactoryDefinition to the clipboard, you can pipe the cmdlet to clip.exe.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmicpuloadsensor | New-SensorFactoryDefinition { $_.Device } 0</code>
    ///     <para>Create a channel definition for the "Total" channel (ID: 0) of each WMI CPU Load sensor in the system</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmimemorysensor | New-SensorFactoryDefinition { $_.Device } -Expr { "100 - $expr" } 0</code>
    ///     <para>Create a channel definition for the "Percent Available Memory" channel (ID: 0) of each WMI Memory Free sensor, modifying each channel to show the percent of memory "used" instead of free</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $sensors = Get-Sensor -Tags wmicpuloadsensor</code>
    ///     <para>C:\> $sensors | New-SensorFactoryDefinition { "Max CPU Load" } -Aggregator { "max($expr,$acc)" }</para>
    ///     <para>C:\> $sensors | New-SensorFactoryDefinition { $_.Device } -StartId 2</para>
    ///     <para>Create a channel definition for showing the highest CPU Load of all sensors as well as channel definitions for each individual sensor</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmicpuloadsensor | New-SensorFactoryDefinition { "$($_.Device) [bananas]" } 0</code>
    ///     <para>Create a channel definition for the "Total" channel (ID: 0) displaying all channels with the custom unit "bananas"</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Channel</para> 
    /// <para type="link">https://www.paessler.com/manuals/prtg/sensor_factory_sensor</para> 
    /// </summary>
    [Cmdlet(VerbsCommon.New, "SensorFactoryDefinition")]
    public class NewSensorFactoryDefinition : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The sensor to create a channel definition for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// <para type="description">An expression that resolves the name to use for a channel definition.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public ScriptBlock Name { get; set; }

        /// <summary>
        /// <para type="description">The channel ID to use. If a custom expression is provided, this value can be optionally overridden.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        public int ChannelId { get; set; }

        /// <summary>
        /// <para type="description">A custom expression to use for defining a channel definition.</para>
        /// <para type="description">Provides the following automatic variables</para>
        /// <para type="description">    '$expr' (for the default expression)</para>
        /// <para type="description">    '$_'    (for the current sensor)</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public ScriptBlock Expression { get; set; }

        /// <summary>
        /// <para type="description">The starting channel ID to use for each channel definition. The default value is 1.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int StartId { get; set; } = 1;

        /// <summary>
        /// <para type="description">An aggregator to use for creating a single aggregation channel.</para>
        /// <para type="description">Provides the fllowing automatic variables:</para>
        /// <para type="description">    '$acc'  (the running accumulator)</para>
        /// <para type="description">    '$expr' (for the default expression)</para>
        /// <para type="description">    '$_'    (the current sensor</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Aggregate")]
        public ScriptBlock Aggregator { get; set; }

        /// <summary>
        /// <para type="description">A post-processing action to perform on an aggregated expresion before emititing to the pipeline.</para>
        /// <para type="description">Provides the following automatic variables:</para>
        /// <para type="description">    '$acc' (the accumulated result)</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Aggregate")]
        public ScriptBlock Finalizer { get; set; }

        private int id;
        private readonly PSVariable accumulation = new PSVariable("acc");

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            id = StartId;
        }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            var rows = new List<string>();

            var expression = GetExpression();

            if (Aggregator != null)
            {
                ProcessAggregtor(expression);
            }
            else
            {
                var name = Name.InvokeWithDollarUnderscore(Sensor).ToString();
                rows.Add($"#{id}:{name}");
                rows.Add(expression);
                WriteObject(rows, true);
                id++;
            }
        }

        private string GetExpression()
        {
            string expression = $"channel({Sensor.Id},{ChannelId})";

            if (Expression != null)
            {
                expression = Expression.InvokeWithContext(null, new List<PSVariable>
                {
                    new PSVariable("expr", expression),
                    new PSVariable("_", Sensor)
                }).First().ToString();
            }

            return expression;
        }

        private void ProcessAggregtor(string expression)
        {
            if (accumulation.Value == null)
            {
                accumulation.Value = expression;
            }
            else
            {
                accumulation.Value = Aggregator.InvokeWithContext(null, new List<PSVariable>
                {
                    accumulation,
                    new PSVariable("_", Sensor),
                    new PSVariable("expr", expression)
                }).First();
            }
        }

        /// <summary>
        /// Provides a one-time, postprocessing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
            if (Aggregator != null)
            {
                if (Finalizer != null)
                {
                    accumulation.Value = Finalizer.InvokeWithContext(null, new List<PSVariable>
                    {
                        accumulation
                    }).First();
                }

                var name = Name.InvokeWithDollarUnderscore(Sensor).ToString();
                WriteObject($"#{id}:{name}");
                WriteObject(accumulation.Value);
            }
        }
    }

    internal static class ScriptBlockHelpers
    {
        internal static object InvokeWithDollarUnderscore(this ScriptBlock scriptBlock, object args)
        {
            var method = scriptBlock.GetType().GetMethod("DoInvokeReturnAsIs", BindingFlags.Instance | BindingFlags.NonPublic);

            var result = method.Invoke(scriptBlock, new[]
            {
                (object)true,                 //useLocalScope
                (object)3,                    //errorhandlingBehavior - WriteToExternalErrorPipe
                (object)args,                 //dollarUnder
                (object)AutomationNull.Value, //input
                (object)AutomationNull.Value, //scriptThis
                (object)new [] { args }       //Args
            });

            if (result == null)
                return null;

            if (result is PSObject)
            {
                var psObj = (PSObject)result;

                if (psObj.ToString() == string.Empty)
                    return null;

                var underlying = psObj.BaseObject;

                return underlying;
            }

            return result;
        }
    }
}
