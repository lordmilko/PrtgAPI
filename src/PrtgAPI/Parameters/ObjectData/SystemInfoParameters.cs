using System.Linq;
using System.Runtime.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    class SystemInfoParameters<T> : BaseActionParameters, IJsonParameters where T : IDeviceInfo
    {
        JsonFunction IJsonParameters.Function => JsonFunction.TableData;

        public SysInfoProperty[] Columns
        {
            get { return (SysInfoProperty[])this[Parameter.Columns]; }
            set { this[Parameter.Columns] = value; }
        }

        public SystemInfoParameters(int deviceId) : base(deviceId)
        {
            this[Parameter.Content] = Content.SysInfo;
            Columns = GetProperties();

            var category = typeof(T).GetTypeCache().GetAttribute<CategoryAttribute>();

            if (category == null)
                throw new MissingAttributeException(GetType(), typeof(CategoryAttribute));

            this[Parameter.Category] = category.Name.GetDescription().ToLower();
        }

        private SysInfoProperty[] GetProperties()
        {
            var properties = typeof(T).GetTypeCache().Properties
                .Where(p => p.GetAttribute<UndocumentedAttribute>() == null)
                .Select(e => e.GetAttribute<DataMemberAttribute>())
                .Where(p => p != null)
                .Select(e => e.Name.ToEnum<SysInfoProperty>())
                .Distinct()
                .ToArray();

            return properties;
        }
    }
}
