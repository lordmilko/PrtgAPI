using System;
using System.Text.RegularExpressions;
using PrtgAPI.Objects.Shared;

namespace PrtgAPI
{
    /// <summary>
    /// An action to be performed by PRTG when a <see cref="NotificationTrigger"/> activates.
    /// </summary>
    public class NotificationAction : PrtgObject, IFormattable //maybe notificationaction inherits from notificationactiondescriptor which is a prtgobject?
        //i dont think EVERY prtgobject has a comment

    //have an integration test that asks for every SINGLE possible property, and confirms that for each property with a value there is a property on the object for it.
    //we have our property enum so we can probably just turn every field in that into an array
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAction"/> class.
        /// </summary>
        public NotificationAction()
        {
        }

        internal NotificationAction(string raw)
        {
            var regex = new Regex("(.+)\\|(.+)"); //noti

            var number = regex.Replace(raw, "$1");
            var name = regex.Replace(raw, "$2");

            Id = Convert.ToInt32(number);
            Name = name;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
            //return $"{Id}|{Name}|"; //i dont think theres actually meant to be the second pipe?
        }

        string IFormattable.GetSerializedFormat()
        {
            return $"{Id}|{Name}";
        }

        /*
         * 
         * http://prtg.example.com/api/table.json?content=notifications&columns=objid,type,name,tags,active,downtime,downtimetime,downtimesince,uptime,uptimetime,uptimesince,knowntime,cumsince,sensor,interval,lastcheck,lastup,lastdown,device,group,probe,grpdev,notifiesx,intervalx,access,dependency,probegroupdevice,status,message,priority,lastvalue,upsens,downsens,downacksens,partialdownsens,warnsens,pausedsens,unusualsens,undefinedsens,totalsens,value,coverage,favorite,user,parent,datetime,dateonly,timeonly,schedule,period,email,template,lastrun,nextrun,size,minigraph,deviceicon,comments,host,condition,basetype,baselink,icon,parentid,location,fold,groupnum,%20devicenum,tickettype,modifiedby,actions,content&count=50000&filter_name=@sub(mem)

        //get-notification - should get the notifications on an object
          get-notificationactions - should get this class, i think. rename to notificationaction?

        we can use the url above to test for secret properties for other content types!

        

notifications

    objid
    type
type_raw
    name
    active
    active_raw
probegroupdevice
probegroupdevice_raw
priority
value channelid
favorite
favorite_raw
datetime
    datetime_raw
dateonly
dateonly_raw
timeonly
timeonly_raw
baselink
baselink_raw
    parentid
fold
fold_raw
actions

         * 
         */
    }
}
