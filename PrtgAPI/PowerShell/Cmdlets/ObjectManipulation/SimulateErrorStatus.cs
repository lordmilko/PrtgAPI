using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    //todo: add some tests

    /// <summary>
    /// <para type="synopsis">Simulates an error status on a PRTG Sensor.</para>
    /// 
    /// <para type="description">The Simulate-ErrorStatus cmdlet forces a PRTG Sensor to enter an error (Down) state.
    /// When an object is in a simulated error status, it will display a message indicating it is in a simulated error state.
    /// Any object put into a simulated error status will remain in this state until the object is resumed via the Resume-Object cmdlet
    /// or via the PRTG UI. Even if an object is paused, it will return to an error state when the object is unpaused.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | Simulate-ErrorStatus</code>
    ///     <para>Force the sensor with ID to enter an error state.</para>
    /// </example>
    /// 
    /// <para type="link">Resume-Object</para>
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "ErrorStatus")]
    public class SimualteErrorStatus : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The sensor to simulate an error status on.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            ExecuteOperation(() => client.SimulateError(Sensor.Id), "Simulating Sensor Errors", $"Processing sensor '{Sensor.Name}'");
        }
    }
}
