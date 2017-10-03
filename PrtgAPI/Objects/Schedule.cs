using System;
using System.Text.RegularExpressions;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a schedule used to indicate when monitoring should be active on an object.
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// The name of the monitoring schedule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The index of the monitoring schedule within PRTG.
        /// </summary>
        public int Index { get; set; }

        internal Schedule(string raw)
        {
            var regex = new Regex("(.+?)\\|(.+?)\\|");

            var number = regex.Replace(raw, "$1");
            var name = regex.Replace(raw, "$2");

            Index = Convert.ToInt32(number);
            Name = name;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
