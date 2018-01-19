using System;
using System.Collections.Generic;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI.PowerShell.Cmdlets
{
    class CloneCmdletConfig
    {
        /// <summary>
        /// The name to give the new object.
        /// </summary>
        public string Name { get; set; }
        public string Host { get; set; }
        public PrtgObject PrtgObject => Object as PrtgObject;
        public object Object { get; set; }
        public Func<int, List<PrtgObject>> GetObjects { get; set; }
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

                return PrtgObject.GetType().Name;
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

                return PrtgObject.Name;
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

                return $"ID: {PrtgObject.Id}";
            }
        }

        public CloneCmdletConfig(object obj, string name, Func<int, List<PrtgObject>> getObjects)
        {
            Name = name;
            Object = obj;
            GetObjects = getObjects;

            if(PrtgObject != null)
                Cloner = (c, d) => c.CloneObject(PrtgObject.Id, Name, d);
        }

        public CloneCmdletConfig(object obj, string name, string host, Func<int, List<PrtgObject>> getObjects)
        {
            Name = name;
            Host = host;
            Object = obj;
            GetObjects = getObjects;

            if(PrtgObject != null)
                Cloner = (c, d) => c.CloneObject(PrtgObject.Id, Name, Host, d);
        }
    }
}