using System;
using System.Collections.Generic;
using System.Xml;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class SensorSerializerManual : XmlSerializerManual
    {
        //Header
        object totalcount;
        object prtgversion;
        object item;

        //Sensor
        object probe;
        object group;
        object favorite;
        object displayLastValue;
        object lastValue;
        object device;
        object downtime;
        object totaldowntime;
        object downduration;
        object uptime;
        object totaluptime;
        object upduration;
        object totalmonitortime;
        object datacollectedsince;
        object lastcheck;
        object lastup;
        object lastdown;
        object minigraph;

        //SensorOrDeviceOrGroupOrProbe

        object schedule;
        object basetype;
        object url;
        object parentid;
        object notificationtypes;
        object interval;
        object inheritinterval;
        object access;
        object dependency;
        object position;
        object status;
        object comments;

        //SensorOrDeviceOrGroupOrProbeOrTicket

        object priority;
        object priorityRaw;

        //SensorOrDeviceOrGroupOrProbeOrTicketOrTicketData

        object message;
        object displayMessage;

        //PrtgObject

        object id;
        object name;
        object tags;
        object injectedTags;
        object displayType;
        object type;
        object active;

        public SensorSerializerManual(XmlReader reader) : base(reader)
        {
        }

        public override object Deserialize(bool validateValueTypes)
        {
            return Deserialize<Sensor>(3, ProcessHeaderAttributes, ProcessHeaderElements, ValidateHeader);
        }

        protected override void Init()
        {
            //Header
            totalcount = reader.NameTable.Add("totalcount");
            prtgversion = reader.NameTable.Add("prtg-version");
            item = reader.NameTable.Add("item");

            //Sensor
            probe = reader.NameTable.Add("probe");
            group = reader.NameTable.Add("group");
            favorite = reader.NameTable.Add("favorite_raw");
            displayLastValue = reader.NameTable.Add("lastvalue");
            lastValue = reader.NameTable.Add("lastvalue_raw");
            device = reader.NameTable.Add("device");
            downtime = reader.NameTable.Add("downtime_raw");
            totaldowntime = reader.NameTable.Add("downtimetime_raw");
            downduration = reader.NameTable.Add("downtimesince_raw");
            uptime = reader.NameTable.Add("uptime_raw");
            totaluptime = reader.NameTable.Add("uptimetime_raw");
            upduration = reader.NameTable.Add("uptimesince_raw");
            totalmonitortime = reader.NameTable.Add("knowntime_raw");
            datacollectedsince = reader.NameTable.Add("cumsince_raw");
            lastcheck = reader.NameTable.Add("lastcheck_raw");
            lastup = reader.NameTable.Add("lastup_raw");
            lastdown = reader.NameTable.Add("lastdown_raw");
            minigraph = reader.NameTable.Add("minigraph");

            //SensorOrDeviceOrGroupOrProbe

            schedule = reader.NameTable.Add("schedule");
            basetype = reader.NameTable.Add("basetype");
            url = reader.NameTable.Add("baselink");
            parentid = reader.NameTable.Add("parentid");
            notificationtypes = reader.NameTable.Add("notifiesx");
            interval = reader.NameTable.Add("intervalx_raw");
            inheritinterval = reader.NameTable.Add("intervalx");
            access = reader.NameTable.Add("access_raw");
            dependency = reader.NameTable.Add("dependency_raw");
            position = reader.NameTable.Add("position");
            status = reader.NameTable.Add("status_raw");
            comments = reader.NameTable.Add("comments");

            //SensorOrDeviceOrGroupOrProbeOrTicket

            priority = reader.NameTable.Add("priority");
            priorityRaw = reader.NameTable.Add("priority_raw");

            //SensorOrDeviceOrGroupOrProbeOrTicketOrTicketData

            message = reader.NameTable.Add("message_raw");
            displayMessage = reader.NameTable.Add("message");

            //PrtgObject

            id = reader.NameTable.Add("objid");
            name = reader.NameTable.Add("name");
            tags = reader.NameTable.Add("tags");
            injectedTags = reader.NameTable.Add("injected_tags");
            displayType = reader.NameTable.Add("type");
            type = reader.NameTable.Add("type_raw");
            active = reader.NameTable.Add("active_raw");
        }

        private bool ProcessHeaderAttributes(TableData<Sensor> obj, bool[] flagArray)
        {
            if (!flagArray[0] && AttributeName == totalcount)
            {
                obj.TotalCount = ToInt32(reader.ReadContentAsString());
                flagArray[0] = true;
                return true;
            }

            return false;
        }

        private bool ProcessHeaderElements(TableData<Sensor> obj, bool[] flagArray)
        {
            if (!flagArray[1] && ElementName == prtgversion)
            {
                obj.Version = reader.ReadElementContentAsString();
                flagArray[1] = true;
                return true;
            }
            else if (ElementName == item)
            {
                if (obj.Items == null)
                    obj.Items = new List<Sensor>();

                if (obj.Items == null)
                    reader.Skip();
                else
                    obj.Items.Add(ReadSensor());

                return true;
            }

            return false;
        }

        private void ValidateHeader(bool[] flagArray)
        {
            if (flagArray[0])
            {
                ElementName = totalcount;
                throw Fail(null, null, typeof(int));
            }
        }

        private Sensor ReadSensor()
        {
            return ReadElement<Sensor>(39, (s, f) => false, ProcessSensorElements, ValidateSensor); //todo: is it really 38?
        }

        private bool ProcessSensorAttributes(Sensor obj, bool[] flagArray)
        {
            return false;
        }

        private bool ProcessSensorElements(Sensor obj, bool[] flagArray)
        {
            //Sensor
            if (!flagArray[0] && ElementName == probe)
            {
                obj.Probe = reader.ReadElementContentAsString();
                flagArray[0] = true;
                return true;
            }
            else if (!flagArray[1] && ElementName == group)
            {
                obj.Group = reader.ReadElementContentAsString();
                flagArray[1] = true;
                return true;
            }
            else if (!flagArray[2] && ElementName == favorite)
            {
                obj.Favorite = ToBool(reader.ReadElementContentAsString());
                flagArray[2] = true;
                return true;
            }
            else if (!flagArray[3] && ElementName == displayLastValue)
            {
                obj.DisplayLastValue = reader.ReadElementContentAsString();
                flagArray[3] = true;
                return true;
            }
            else if (!flagArray[4] && ElementName == lastValue)
            {
                obj.LastValue = ToNullableDouble(reader.ReadElementContentAsString());
                flagArray[4] = true;
                return true;
            }
            else if (!flagArray[5] && ElementName == device)
            {
                obj.Device = reader.ReadElementContentAsString();
                flagArray[5] = true;
                return true;
            }
            else if (!flagArray[6] && ElementName == downtime)
            {
                obj.Downtime = ToNullableDouble(reader.ReadElementContentAsString());
                flagArray[6] = true;
                return true;
            }
            else if (!flagArray[7] && ElementName == totaldowntime)
            {
                obj.TotalDowntime = ToNullableTimeSpan(reader.ReadElementContentAsString());
                flagArray[7] = true;
                return true;
            }
            else if (!flagArray[8] && ElementName == downduration)
            {
                obj.DownDuration = ToNullableTimeSpan(reader.ReadElementContentAsString());
                flagArray[8] = true;
                return true;
            }
            else if (!flagArray[9] && ElementName == uptime)
            {
                obj.Uptime = ToNullableDouble(reader.ReadElementContentAsString());
                flagArray[9] = true;
                return true;
            }
            else if (!flagArray[10] && ElementName == totaluptime)
            {
                obj.TotalUptime = ToNullableTimeSpan(reader.ReadElementContentAsString());
                flagArray[10] = true;
                return true;
            }
            else if (!flagArray[11] && ElementName == upduration)
            {
                obj.UpDuration = ToNullableTimeSpan(reader.ReadElementContentAsString());
                flagArray[11] = true;
                return true;
            }
            else if (!flagArray[12] && ElementName == totalmonitortime)
            {
                obj.TotalMonitorTime = ToTimeSpan(reader.ReadElementContentAsString());
                flagArray[12] = true;
                return true;
            }
            else if (!flagArray[13] && ElementName == datacollectedsince)
            {
                obj.DataCollectedSince = ToNullableDateTime(reader.ReadElementContentAsString());
                flagArray[13] = true;
                return true;
            }
            else if (!flagArray[14] && ElementName == lastcheck)
            {
                obj.LastCheck = ToNullableDateTime(reader.ReadElementContentAsString());
                flagArray[14] = true;
                return true;
            }
            else if (!flagArray[15] && ElementName == lastup)
            {
                obj.LastUp = ToNullableDateTime(reader.ReadElementContentAsString());
                flagArray[15] = true;
                return true;
            }
            else if (!flagArray[16] && ElementName == lastdown)
            {
                obj.LastDown = ToNullableDateTime(reader.ReadElementContentAsString());
                flagArray[16] = true;
                return true;
            }
            else if (!flagArray[17] && ElementName == minigraph)
            {
                obj.MiniGraph = reader.ReadElementContentAsString();
                flagArray[17] = true;
                return true;
            }

            //SensorOrDeviceOrGroupOrProbe
            else if (!flagArray[18] && ElementName == schedule)
            {
                obj.Schedule = reader.ReadElementContentAsString();
                flagArray[18] = true;
                return true;
            }
            else if (!flagArray[19] && ElementName == basetype)
            {
                obj.BaseType = ReadBaseType(reader.ReadElementContentAsString());
                flagArray[19] = true;
                return true;
            }
            else if (!flagArray[20] && ElementName == url)
            {
                obj.Url = reader.ReadElementContentAsString();
                flagArray[20] = true;
                return true;
            }
            else if (!flagArray[21] && ElementName == parentid)
            {
                obj.ParentId = ToInt32(reader.ReadElementContentAsString());
                flagArray[21] = true;
                return true;
            }
            else if (!flagArray[22] && ElementName == notificationtypes)
            {
                obj.notificationTypes = reader.ReadElementContentAsString();
                flagArray[22] = true;
                return true;
            }
            else if (!flagArray[23] && ElementName == interval)
            {
                obj.Interval = ToTimeSpan(reader.ReadElementContentAsString());
                flagArray[23] = true;
                return true;
            }
            else if (!flagArray[24] && ElementName == inheritinterval)
            {
                obj.inheritInterval = reader.ReadElementContentAsString();
                flagArray[24] = true;
                return true;
            }
            else if (!flagArray[25] && ElementName == access)
            {
                obj.Access = ReadAccess(reader.ReadElementContentAsString());
                flagArray[25] = true;
                return true;
            }
            else if (!flagArray[26] && ElementName == dependency)
            {
                obj.Dependency = reader.ReadElementContentAsString();
                flagArray[26] = true;
                return true;
            }
            else if (!flagArray[27] && ElementName == position)
            {
                obj.Position = ToInt32(reader.ReadElementContentAsString());
                flagArray[27] = true;
                return true;
            }
            else if (!flagArray[28] && ElementName == status)
            {
                obj.Status = ReadStatus(reader.ReadElementContentAsString());
                flagArray[28] = true;
                return true;
            }
            else if (!flagArray[29] && ElementName == comments)
            {
                obj.Comments = reader.ReadElementContentAsString();
                flagArray[29] = true;
                return true;
            }

            //SensorOrDeviceOrGroupOrProbeOrTicket

            else if (!flagArray[30] && (ElementName == priority) || ElementName == priorityRaw)
            {
                obj.Priority = ReadPriority(reader.ReadElementContentAsString());
                flagArray[30] = true;
                return true;
            }

            //SensorOrDeviceOrGroupOrProbeOrTicketOrTicketData

            else if (!flagArray[31] && ElementName == message)
            {
                obj.Message = reader.ReadElementContentAsString();
                flagArray[31] = true;
                return true;
            }
            else if (!flagArray[32] && ElementName == displayMessage)
            {
                obj.DisplayMessage = reader.ReadElementContentAsString();
                flagArray[32] = true;
                return true;
            }

            //PrtgObject

            else if (!flagArray[33] && ElementName == id)
            {
                obj.Id = ToInt32(reader.ReadElementContentAsString());
                flagArray[33] = true;
                return true;
            }
            else if (!flagArray[34] && ElementName == ElementName)
            {
                obj.Name = reader.ReadElementContentAsString();
                flagArray[34] = true;
                return true;
            }
            else if (!flagArray[35] && (ElementName == tags || ElementName == injectedTags))
            {
                obj.Tags = ToSplittableStringArray(reader.ReadElementContentAsString(), ' ');
                flagArray[35] = true;
                return true;
            }
            else if (!flagArray[36] && ElementName == displayType)
            {
                obj.DisplayType = reader.ReadElementContentAsString();
                flagArray[36] = true;
                return true;
            }
            else if (!flagArray[37] && ElementName == type)
            {
                obj.type = reader.ReadElementContentAsString();
                flagArray[37] = true;
                return true;
            }
            else if (!flagArray[38] && ElementName == active)
            {
                obj.Active = ToBool(reader.ReadElementContentAsString());
                flagArray[38] = true;
                return true;
            }

            return false;
        }

        private void ValidateSensor(bool[] flagArray)
        {
            //Sensor
            if (flagArray[2])
            {
                ElementName = favorite;
                throw Fail(null, null, typeof(bool));
            }
            else if (flagArray[12])
            {
                ElementName = totalmonitortime;
                throw Fail(null, null, typeof(TimeSpan));
            }

            //SensorOrDeviceOrGroupOrProbe
            
            else if (flagArray[19])
            {
                ElementName = basetype;
                throw Fail(null, null, typeof(BaseType));
            }
            else if (flagArray[21])
            {
                ElementName = parentid;
                throw Fail(null, null, typeof(int));
            }
            else if (flagArray[23])
            {
                ElementName = interval;
                throw Fail(null, null, typeof(TimeSpan));
            }
            else if (flagArray[25])
            {
                ElementName = access;
                throw Fail(null, null, typeof(Access));
            }
            else if (flagArray[27])
            {
                ElementName = position;
                throw Fail(null, null, typeof(int));
            }
            else if (flagArray[28])
            {
                ElementName = status;
                throw Fail(null, null, typeof(Status));
            }

            //SensorOrDeviceOrGroupOrProbeOrTicket

            else if (flagArray[30] && (ElementName == priority) || ElementName == priorityRaw)
            {
                ElementName = priority;
                throw Fail(null, null, typeof(Priority));
            }

            //PrtgObject

            else if (flagArray[33])
            {
                ElementName = id;
                throw Fail(null, null, typeof(int));
            }
            else if (flagArray[38])
            {
                ElementName = active;
                throw Fail(null, null, typeof(bool));
            }
        }

        private BaseType ReadBaseType(string s)
        {
            switch(s)
            {
                case "sensor":
                    return BaseType.Sensor;
                case "device":
                    return BaseType.Device;
                case "group":
                    return BaseType.Group;
                case "probe":
                    return BaseType.Probe;
                default:
                    throw new NotImplementedException(); //todo: need to call unhandled string
            }
        }

        private Access ReadAccess(string s)
        {
            switch(s)
            {
                case "-1":
                case "-0000000001":
                    return Access.Inherited;
                case "0":
                case "0000000000":
                    return Access.None;
                case "100":
                case "0000000100":
                    return Access.Read;
                case "200":
                case "0000000200":
                    return Access.Write;
                case "400":
                case "0000000400":
                    return Access.Full;
                case "Admin":
                    return Access.Admin;
                default:
                    throw FailEnum(s, typeof(Access));
            }
        }

        private Priority ReadPriority(string s)
        {
            switch(s)
            {
                case "0":
                    return Priority.None;
                case "1":
                    return Priority.One;
                case "2":
                    return Priority.Two;
                case "3":
                    return Priority.Three;
                case "4":
                    return Priority.Four;
                case "5":
                    return Priority.Five;
                default:
                    throw FailEnum(s, typeof(Priority));
            }

            throw new NotImplementedException();
        }

        private Status ReadStatus(string s)
        {
            switch(s)
            {
                case "0":
                    return Status.None;
                case "1":
                    return Status.Unknown;
                case "2":
                    return Status.Collecting;
                case "3":
                    return Status.Up;
                case "4":
                    return Status.Warning;
                case "5":
                    return Status.Down;
                case "6":
                    return Status.NoProbe;
                case "7":
                    return Status.PausedByUser;
                case "8":
                    return Status.PausedByDependency;
                case "9":
                    return Status.PausedBySchedule;
                case "10":
                    return Status.Unusual;
                case "11":
                    return Status.PausedByLicense;
                case "12":
                    return Status.PausedUntil;
                case "13":
                    return Status.DownAcknowledged;
                case "14":
                    return Status.DownPartial;
                default:
                    throw FailEnum(s, typeof(Status));
            }
        }
    }
}
