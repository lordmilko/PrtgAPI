using System;
using System.Collections.Generic;

namespace PrtgAPI.PowerShell
{
    class CloneCmdletConfig
    {
        /// <summary>
        /// The name to give the new object.
        /// </summary>
        public string Name { get; set; }
        public string Host { get; set; }
        public IObject IObject => Object as IObject;
        public object Object { get; set; }
        public Func<int, List<IObject>> GetObjects { get; set; }
        public Func<PrtgClient, int, int> Cloner { get; set; }

        public bool AllowBasic { get; set; }

        public string TypeDescription
        {
            get
            {
                if (Object is NotificationTrigger)
                {
                    return "Trigger";
                }

                return IObject.GetType().Name;
            }
        }

        public string NameDescrption
        {
            get
            {
                if (Object is NotificationTrigger)
                {
                    return ((NotificationTrigger) Object).OnNotificationAction.ToString();
                }

                return IObject.Name;
            }
        }

        public string IdDescription
        {
            get
            {
                var trigger = Object as NotificationTrigger;

                if (trigger != null)
                {
                    return $"ID: {trigger.ObjectId}, SubID: {trigger.SubId}";
                }

                return $"ID: {IObject.GetId()}";
            }
        }

        public CloneCmdletConfig(object obj, string name, Func<int, List<IObject>> getObjects)
        {
            Name = name;
            Object = obj;
            GetObjects = getObjects;

            if (IObject != null)
                Cloner = (c, d) => c.CloneObject(IObject.GetId(), Name, d);
        }

        public CloneCmdletConfig(object obj, string name, string host, Func<int, List<IObject>> getObjects)
        {
            Name = name;
            Host = host;
            Object = obj;
            GetObjects = getObjects;

            if (IObject != null)
                Cloner = (c, d) => c.CloneObject(IObject.GetId(), Name, Host, d);
        }
    }
}