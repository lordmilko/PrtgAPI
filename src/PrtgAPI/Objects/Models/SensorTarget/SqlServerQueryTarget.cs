using System;
using System.Collections.Generic;

namespace PrtgAPI.Targets
{
    /// <summary>
    /// Represents a SQL query file that can be used to query the status of a SQL Server Database.
    /// </summary>
    public class SqlServerQueryTarget : SensorTarget<SqlServerQueryTarget>
    {
        /// <summary>
        /// Converts an object of one of several types to a <see cref="SqlServerQueryTarget"/>. If the specified value is not convertable to <see cref="SqlServerQueryTarget"/>, an <see cref="InvalidCastException"/> is thrown.
        /// </summary>
        /// <param name="sqlQuery">The value to parse.</param>
        /// <exception cref="InvalidCastException">The specified value could not be parsed to a <see cref="SqlServerQueryTarget"/>.</exception>
        /// <returns>An <see cref="SqlServerQueryTarget"/> that encapsulates the passed value.</returns>
        public static SqlServerQueryTarget Parse(object sqlQuery)
        {
            return ParseStringCompatible(sqlQuery);
        }

        private SqlServerQueryTarget(string raw) : base(raw)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SqlServerQueryTarget"/> from a specified file name.<para/>
        /// Note: PrtgAPI does not verify that the specified file exists on the probe of the target device.<para/>
        /// If the specified name is invalid, any sensors created will show an error stating the file does not exist.
        /// </summary>
        /// <param name="sqlQuery">The name of the query file to use.</param>
        public static implicit operator SqlServerQueryTarget(string sqlQuery)
        {
            if (sqlQuery == null)
                return null;

            return new SqlServerQueryTarget(ToDropDownOption(sqlQuery));
        }

        internal static List<SqlServerQueryTarget> GetQueries(string response)
        {
            return CreateFromDropDownOptions(response, ObjectProperty.SqlServerQuery, o => new SqlServerQueryTarget(o));
        }
    }
}
