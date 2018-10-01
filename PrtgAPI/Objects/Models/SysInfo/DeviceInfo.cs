using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using PrtgAPI.Helpers;

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
            intField = Convert.ToInt32(value);
        }

        internal void SetLong(string value, ref string strField, ref long? longField)
        {
            if (string.IsNullOrEmpty(value))
                return;

            strField = value;
            longField = Convert.ToInt64(value);
        }

        internal void SetDate(string value, ref string strField, ref DateTime? dateField)
        {
            if (string.IsNullOrEmpty(value))
                return;

            strField = value;
            dateField = FormatDate(value);
        }

        internal void SetVersion(string value, ref string strField, ref Version versionField)
        {
            if (string.IsNullOrEmpty(value))
                return;

            strField = value;

            if (!value.Contains("."))
                value += ".0";

            versionField = Version.Parse(value);
        }

        internal string CleanState(string state)
        {
            var img = Regex.Match(state, "<img.+> *");

            if (img.Success)
                return state.Replace(img.Value, "");

            return state;
        }

        internal virtual DateTime FormatDate(string value)
        {
            return ParameterHelpers.StringToDate(value);
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
