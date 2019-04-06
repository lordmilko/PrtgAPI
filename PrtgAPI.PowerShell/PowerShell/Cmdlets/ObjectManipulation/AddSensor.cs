using System;
using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adds a new sensor to a PRTG Device.</para>
    /// 
    /// <para type="description">The Add-Sensor cmdlet adds a new sensor to a PRTG Device. When adding a new
    /// sensor, you must first create a <see cref="NewSensorParameters"/> object that defines the type of
    /// sensor to create and the settings to use in the new object.</para>
    /// 
    /// <para type="description">When adding sensor types that are natively supported by PrtgAPI, Add-Sensor
    /// will validate that all mandatory parameter fields contain values. If a mandatory field is missing
    /// a value, Add-Sensor will throw an <see cref="InvalidOperationException"/>, listing the field whose value was missing.
    /// When adding unsupported sensor types defined in <see cref="RawSensorParameters"/>, Add-Sensor does not
    /// perform any parameter validation. As such, it is critical to ensure that all parameter names and values
    /// are valid before passing the parameters to the Add-Sensor cmdlet.</para>
    /// 
    /// <para type="description">For parameter types that support specifying multiple SensorTarget objects
    /// (such as <see cref="WmiServiceSensorParameters"/>) PRTG can fail to add all sensors properly if the size
    /// of a single request is too large (generally 60 items or greater). To prevent this issue from happening,
    /// PrtgAPI automatically splits your request into a series of smaller requests, however if you find yourself
    /// experiencing issues, this is something to be aware of.</para>
    /// 
    /// <para type="description">By default, Add-Sensor will attempt to resolve the created sensor(s) to one
    /// or more <see cref="Sensor"/> objects. As PRTG does not return the ID of the created object, PrtgAPI
    /// identifies the newly created group by comparing the sensors under the parent object before and after the new sensor(s) are created.
    /// While this is generally very reliable, in the event something or someone else creates another new sensor directly
    /// under the target object with the same Name, that object will also be returned in the objects
    /// resolved by Add-Sensor. If you do not wish to resolve the created sensor, this behavior can be
    /// disabled by specifying -Resolve:$false.</para>
    /// 
    /// <example>
    ///     <code>
    ///         C:\> $params = New-SensorParameters ExeXml "Monitor Traffic" "TrafficMonitor.ps1"
    ///
    ///         C:\> Get-Device *fw* | Add-Sensor $params
    ///     </code>
    ///     <para>Add an EXE/Script Advanced sensor to all firewall devices, using the script "TrafficMonitor.ps1"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $raw = @{
    ///         >>      name_ = "my raw sensor"
    ///         >>      tags_ = "xmlexesensor"
    ///         >>      priority_ = 4
    ///         >>      exefile_ = "CustomScript.ps1|CustomScript.ps1||
    ///         >>      exeparams_ = "arg1 arg2 arg3"
    ///         >>      environment_ = 1
    ///         >>      usewindowsauthentication_ = 1
    ///         >>      mutexname_ = "testMutex"
    ///         >>      timeout_ = 70
    ///         >>      writeresult_ = 1
    ///         >>      intervalgroup = 0
    ///         >>      interval_ = "30|30 seconds"
    ///         >>      errorintervalsdown_ = 2
    ///         >>      sensortype = "exexml"
    ///         >> }
    /// 
    ///         C:\> $params = New-SensorParameters $raw
    /// 
    ///         C:\> Get-Device dc-1 | Add-Sensor $params
    ///     </code>
    ///     <para>Add a new EXE/Script Advanced sensor to the device named dc-1 using a hashtable containing its raw parameters.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Creation#creation">Online version:</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">New-SensorParameters</para>
    /// <para type="linl">New-Sensor</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "Sensor", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class AddSensor : AddParametersObject<NewSensorParameters, Sensor, Device>
    {
        /// <summary>
        /// <para type="description">A set of parameters whose properties describe the type of object to add, with what settings.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Target)]
        public new NewSensorParameters Parameters
        {
            get { return base.Parameters; }
            set { base.Parameters = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddSensor"/> class.
        /// </summary>
        public AddSensor() : base(BaseType.Sensor)
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.Default:
                    base.ProcessRecordEx();
                    break;
                case ParameterSet.Target:
                    AddSensorToTarget();
                    break;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }

        private void AddSensorToTarget()
        {
            var internalParams = Parameters as ISourceParameters<Device>;

            if (internalParams?.Source != null)
                AddObjectInternal(internalParams.Source);
            else
                throw new InvalidOperationException("Only sensor parameters created by Get-SensorTarget can be piped to Add-Sensor. Please use 'Default' parameter set, specifying both -Destination and -Parameters.");
        }

        /// <summary>
        /// Resolves the children of the destination object that match the new object's name.
        /// </summary>
        /// <param name="filters">An array of search filters used to retrieve all children of the destination with the specified name.</param>
        /// <returns>All objects under the parent object that match the new object's name.</returns>
        protected override List<Sensor> GetObjects(SearchFilter[] filters) => client.GetSensors(filters);
    }
}
