using System.Collections.Generic;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves all Monitoring Schedules from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-PrtgSchedule cmdlet retrieves all monitoring schedules present on a PRTG Server.
    /// Schedules define the timeframes during which monitoring on sensors can be active. When sensors are deactivated
    /// due to schedules, their statuses will automatically be set to <see cref="Status.PausedBySchedule"/>.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-PrtgSchedule</code>
    ///     <para>Get all monitoring schedules.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-PrtgSchedule *night*</code>
    ///     <para>Get all monitoring schedules whose name contains "night"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $schedule = Get-PrtgSchedule -Id 621</code>
    ///     <para>C:\> Get-Sensor -Id 2024 | Set-ObjectProperty -RawParameters @{</para>
    ///     <para>C:\>     scheduledependency = 0</para>
    ///     <para>C:\>     schedule_ = $schedule</para>
    ///     <para>C:\> }</para>
    ///     <para>Apply the schedule with ID 621 to the sensor with ID 2024</para>
    /// </example>
    /// </summary>
    [OutputType(typeof(Schedule))]
    [Cmdlet(VerbsCommon.Get, "PrtgSchedule")]
    public class GetPrtgSchedule : PrtgTableFilterCmdlet<Schedule, ScheduleParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetPrtgSchedule"/> class.
        /// </summary>
        public GetPrtgSchedule() : base(Content.Schedules, null)
        {
        }

        internal override List<Schedule> GetObjectsInternal(ScheduleParameters parameters)
        {
            return client.GetSchedulesInternal(parameters);
        }

        /// <summary>
        /// Creates a new parameter object to be used for retrieving schedules from a PRTG Server.
        /// </summary>
        /// <returns>The default set of parameters.</returns>
        protected override ScheduleParameters CreateParameters() => new ScheduleParameters();
    }
}
