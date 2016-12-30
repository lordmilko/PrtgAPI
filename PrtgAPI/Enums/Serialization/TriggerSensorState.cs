using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies sensor statuses that can cause a trigger to activate.
    /// </summary>
    public enum TriggerSensorState
    {
        /// <summary>
        /// Trigger when the sensor is <see cref="SensorStatus.Down"/>.
        /// </summary>
        Down = 0,

        /// <summary>
        /// Trigger when the sensor is <see cref="SensorStatus.Warning"/>.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Trigger when the sensor is <see cref="SensorStatus.Unusual"/>.
        /// </summary>
        Unusual = 2,

        /// <summary>
        /// Trigger when the sensor is <see cref="SensorStatus.DownPartial"/>.
        /// </summary>
        PartialDown = 3
    }
}
