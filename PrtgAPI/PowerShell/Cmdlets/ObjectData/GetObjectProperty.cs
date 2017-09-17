using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves properties and settings of a PRTG Object.</para>
    /// 
    /// <para type="description">The Get-ObjectProperty cmdlet retrieves properties and settings of a PRTG Object, as found on the "Settings"
    /// tab within the PRTG UI. Properties retrieved by Get-ObjectProperty may not necessarily be active depending on the value
    /// of their dependent property (e.g. the Interval will not take effect if InheritInterval is $true).</para>
    /// <para type="description">Properties that are not currently supported by Get-ObjectProperty can be retrieved by specifying
    /// the -RawProperty parameter. Raw property names can be found by inspecting the "name" attribute of  the &lt;input/&gt; tag
    /// under the object's Settings page in the PRTG UI. Unlike Set-ObjectProperty, raw property names do not need to include
    /// their trailing underscores when used with Get-ObjectProperty. If a trailing underscore is used, PrtgAPI will automatically truncate it.
    /// When retrieving raw properties please note that PRTG does not support the retrieval of Inheritance related properties via raw lookups.</para>
    /// <para type="description">In order to provide type safety when modifying properties, all properties supported by Set-ObjectProperty
    /// perform a lookup against their corresponding property in Get-ObjectProperty. If the type of value passed to Set-ObjectProperty
    /// does not match the property's expected type, PrtgAPI will attempt to parse the value into the expected type. For more information,
    /// see Set-ObjectProperty.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device -Id 1001 | Get-ObjectProperty</code>
    ///     <para>Retrieve all properties and settings of the device with ID 1001</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 1001 | Get-ObjectProperty -RawProperty name_</code>
    ///     <para>Retrieve the value of raw property "name"</para>
    /// </example>
    /// 
    /// <para type="link">Set-ObjectProperty</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ObjectProperty")]
    public class GetObjectProperty : PrtgCmdlet
    {
        /// <summary>
        /// <para type="description">The object to retrieve properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">The raw name of the property to retrieve. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// Note: PRTG does not support retrieving raw section inheritance settings.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Raw")]
        public string RawProperty { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == "Raw")
            {
                WriteObject(client.GetObjectPropertyRaw(Object.Id, RawProperty));
            }
            else
            {
                switch (Object.BaseType)
                {
                    case BaseType.Sensor:
                        WriteObject(client.GetSensorProperties(Object.Id));
                        break;
                    case BaseType.Device:
                        WriteObject(client.GetDeviceProperties(Object.Id));
                        break;
                    case BaseType.Group:
                        WriteObject(client.GetGroupProperties(Object.Id));
                        break;
                    case BaseType.Probe:
                        WriteObject(client.GetProbeProperties(Object.Id));
                        break;
                }
            }
        }
    }
}
