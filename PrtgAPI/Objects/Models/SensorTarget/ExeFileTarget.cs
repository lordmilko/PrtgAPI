using System;
using System.Collections.Generic;

namespace PrtgAPI.Targets
{
    /// <summary>
    /// Represents an EXE or Script file that can be used for implementing a custom PRTG Sensor.
    /// </summary>
    public class ExeFileTarget : SensorTarget<ExeFileTarget>
    {
        /// <summary>
        /// Converts an object of one of several types to a <see cref="ExeFileTarget"/>. If the specified value is not convertable to <see cref="ExeFileTarget"/>, an <see cref="InvalidCastException"/> is thrown.
        /// </summary>
        /// <param name="exeFile">The value to parse.</param>
        /// <exception cref="InvalidCastException">The specified value could not be parsed to an <see cref="ExeFileTarget"/>.</exception>
        /// <returns>An <see cref="ExeFileTarget"/> that encapsulates the passed value.</returns>
        public static ExeFileTarget Parse(object exeFile)
        {
            return ParseStringCompatible(exeFile);
        }

        /// <summary>
        /// Gets the type of executable file this file targets.
        /// </summary>
        public ExeType Type { get; }

        private ExeFileTarget(string raw) : base(raw)
        {
            if (Name.ToLower().EndsWith("exe"))
                Type = ExeType.Application;
            else
                Type = ExeType.Script;
        }

        /// <summary>
        /// Creates a new <see cref="ExeFileTarget"/> from a specified file name.<para/>
        /// Note: PrtgAPI does not verify that the specified file exists on the probe of the target device.<para/>
        /// If the specified name is invalid, any sensors created will show an error stating the file does not exist.
        /// </summary>
        /// <param name="exeFile">The name of the script or executable to use.</param>
        public static implicit operator ExeFileTarget(string exeFile)
        {
            if (exeFile == null)
                return null;

            return new ExeFileTarget(ToDropDownOption(exeFile));
        }

        internal static List<ExeFileTarget> GetFiles(string response)
        {
            return CreateFromDropDownOptions(response, ObjectProperty.ExeFile, o => new ExeFileTarget(o));
        }
    }
}
