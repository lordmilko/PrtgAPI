using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Represents an abstract piece of system information.
    /// </summary>
    [DataContract]
    public abstract class DeviceInfo : IDeviceInfo
    {
        /// <summary>
        /// ID of the device this information applies to.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Type of system information represented by this object.
        /// </summary>
        public abstract DeviceInfoType Type { get; }

        #region LastUpdated

        private string lastUpdatedStr;
        private DateTime? lastUpdated;

        [DataMember(Name = nameof(SysInfoProperty.ReceiveTime))]
        internal string LastUpdatedStr
        {
            get { return lastUpdatedStr; }
            set { SetDateColon(value, ref lastUpdatedStr, ref lastUpdated, "dd-MM-yyyy HH:mm:ss.FFF"); }
        }

        /// <summary>
        /// Time this information was last received by PRTG from the target device
        /// </summary>
        public DateTime LastUpdated => lastUpdated.Value;

        #endregion
        #region DisplayName

        private string displayNameStr;

        [DataMember(Name = nameof(SysInfoProperty.DisplayName))]
        internal string DisplayNameStr
        {
            get { return displayNameStr; }
            set { SetString(value, ref displayNameStr); }
        }

        /// <summary>
        /// Display name of this object.
        /// </summary>
        public string DisplayName => displayNameStr;

        #endregion

        /// <summary>
        /// Name of this object.
        /// </summary>
        public abstract string Name { get; }

        internal void SetString(string value, ref string field)
        {
            if (string.IsNullOrEmpty(value))
                return;

            field = value;
        }

        internal void SetInt(string value, ref string strField, ref int? intField)
        {
            if (string.IsNullOrEmpty(value))
                return;

            strField = value;

            int i;

            if (int.TryParse(value, out i))
                intField = i;
        }

        internal void SetLong(string value, ref string strField, ref long? longField)
        {
            if (string.IsNullOrEmpty(value))
                return;

            strField = value;

            long l;

            if (long.TryParse(value, out l))
                longField = l;
        }

        internal void SetDateDash(string value, ref string strField, ref DateTime? dateField)
        {
            SetDateInternal(value, ref strField, ref dateField, s => TypeHelpers.StringToDate(s));
        }

        internal void SetDateColon(string value, ref string strField, ref DateTime? dateField, string format = "yyyy-MM-dd HH:mm:ss")
        {
            SetDateInternal(value, ref strField, ref dateField, s =>
            {
                DateTime date;

                //Date could potentially be a string like "Unavailable"
                if (DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    return date;

                return null;
            });
        }

        private void SetDateInternal(string value, ref string strField, ref DateTime? dateField, Func<string, DateTime?> func)
        {
            if (string.IsNullOrEmpty(value))
                return;

            strField = value;
            dateField = func(value);
        }

        internal void SetVersion(string value, ref string strField, ref Version versionField)
        {
            if (string.IsNullOrEmpty(value))
                return;

            strField = value;

            if (!value.Contains("."))
                value += ".0";

            Version v;

            if (Version.TryParse(value, out v))
                versionField = v;
        }

        internal string CleanState(string state)
        {
            var img = Regex.Match(state, "<img.+> *");

            if (img.Success)
                return state.Replace(img.Value, "");

            return state;
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
