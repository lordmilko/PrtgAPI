using System;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    class BaseObjectSettingParameters<T> : BaseActionParameters
    {
        protected BaseObjectSettingParameters(int objectId, T name) : base(objectId)
        {
            ObjectId = objectId;
            Name = name;
        }

        public T Name
        {
            get
            {
                string str = this[Parameter.Name] as string;

                if (str != null)
                {
                    return str.DescriptionToEnum<T>();
                }

                return (T)this[Parameter.Name];
            }
            set
            {
                if (typeof (T).IsEnum)
                {
                    this[Parameter.Name] = (value as Enum).GetDescription();
                }
                else
                {
                    this[Parameter.Name] = value;
                }
            }
        }
    }
}
