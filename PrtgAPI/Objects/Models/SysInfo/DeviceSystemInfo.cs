using System.Diagnostics;
using System.Runtime.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Represents core system snformation of a device.
    /// </summary>
    [DataContract]
    [Category(SystemInfoType.System)]
    [DebuggerDisplay("{Property,nq} = {Value,nq}")]
    public class DeviceSystemInfo : DeviceInfo
    {
        /// <summary>
        /// Type of system information represented by this object.
        /// </summary>
        public override DeviceInfoType Type => DeviceInfoType.System;

        /// <summary>
        /// Name of the property.
        /// </summary>
        public override string Name
        {
            get
            {
                if (Adapter != null)
                    return Adapter;

                return Property;
            }
        }

        #region Property

        private string propertyStr;

        [DataMember(Name = nameof(SysInfoProperty.Key))]
        private string PropertyStr
        {
            get { return propertyStr; }
            set
            {
                if (adapterStr != null && !string.IsNullOrEmpty(value))
                    UpdateProperty(adapterStr, value);

                SetString(value, ref propertyStr);
            }
        }

        /// <summary>
        /// Name of the sub-property (if applicable).
        /// </summary>
        public string Property => propertyStr;

        #endregion
        #region Value

        private string valueStr;

        [DataMember(Name = nameof(SysInfoProperty.Value))]
        private string ValueStr
        {
            get { return valueStr; }
            set { SetString(value, ref valueStr); }
        }

        /// <summary>
        /// Value of the property.
        /// </summary>
        public string Value => valueStr;

        #endregion
        #region Id

        private string idStr;
        private int? id;

        [DataMember(Name = nameof(SysInfoProperty.Id))]
        private string IdStr
        {
            get { return idStr; }
            set { SetInt(value, ref idStr, ref id); }
        }

        /// <summary>
        /// Instance of the property.
        /// </summary>
        public int? Id => id;

        #endregion
        #region Adapter

        private string adapterStr;

        [DataMember(Name = nameof(SysInfoProperty.Adapter))]
        private string AdapterStr
        {
            get { return adapterStr; }
            set
            {
                if (propertyStr != null && !string.IsNullOrEmpty(value))
                    UpdateProperty(value, propertyStr);

                SetString(value, ref adapterStr);
            }
        }

        private void UpdateProperty(string adapter, string property)
        {
            var index = property.IndexOf($" / {adapter}");

            if (index != -1)
            {
                var newVal = Property.Substring(0, index);
                SetString(newVal, ref propertyStr);
            }
        }

        /// <summary>
        /// Adapter name.
        /// </summary>
        public string Adapter => adapterStr;

        #endregion
    }
}
