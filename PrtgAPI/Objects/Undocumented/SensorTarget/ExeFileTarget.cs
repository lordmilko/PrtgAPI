using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Objects.Undocumented;

namespace PrtgAPI
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
        /// <returns>An <see cref="ExeFileTarget"/> that encapsulates the passed value.</returns>
        public static ExeFileTarget Parse(object exeFile)
        {
            if (exeFile == null)
                throw new ArgumentNullException(nameof(exeFile));

            if (exeFile is ExeFileTarget)
                return (ExeFileTarget) exeFile;

            if (exeFile is string)
                return exeFile.ToString();

            throw new InvalidCastException($"Cannot convert '{exeFile}' of type '{exeFile.GetType()}' to type '{nameof(ExeFileTarget)}'. Value type must be convertable to type {typeof(ExeFileTarget).FullName}.");
        }

        /// <summary>
        /// The type of executable file this file targets.
        /// </summary>
        public ExeType Type { get; }

        private ExeFileTarget(string raw) : base(raw)
        {
            Name = components[0];

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
            return new ExeFileTarget($"{exeFile}|{exeFile}||");
        }

        internal static List<ExeFileTarget> GetFiles(string response)
        {
            var files = ObjectSettings.GetDropDownList(response).First(d => d.Name == "exefile").Options.Select(o => new ExeFileTarget(o.Value)).ToList();

            return files;
        }
    }
}
