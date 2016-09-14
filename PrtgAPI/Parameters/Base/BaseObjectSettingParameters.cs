using System;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    class BaseObjectSettingParameters<T> : Parameters
    {
        protected BaseObjectSettingParameters(int objectId, T name)
        {
            ObjectId = objectId;
            Name = name;
        }

        public int ObjectId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
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
