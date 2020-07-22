using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.NotificationActions;
using PrtgAPI.Request;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">An action to be performed by PRTG when a <see cref="NotificationTrigger"/> activates.</para>
    /// </summary>
    [AlternateDescription("Notification Action")]
    public class NotificationAction : PrtgObject, ISerializable, ILazy
    {
        private string url;

        /// <summary>
        /// URL of this object.
        /// </summary>
        [XmlElement("baselink")]
        [PropertyParameter(Property.Url)]
        public string Url
        {
            get { return Lazy(() => url); }
            set { url = value; }
        }

        #region Category Definitions

        internal const string CategoryEmail = "category_email";
        internal const string CategoryPush = "category_push";
        internal const string CategorySMS = "category_sms";
        internal const string CategoryEventLog = "category_eventlog";
        internal const string CategorySyslog = "category_syslog";
        internal const string CategorySNMP = "category_snmp";
        internal const string CategoryHttp = "category_http";
        internal const string CategoryProgram = "category_program";
        internal const string CategoryAmazon = "category_amazon";
        internal const string CategoryTicket = "category_ticket";

        #endregion
        #region Basic Notification Settings

        private bool? postpone { get; set; }

        /// <summary>
        /// Whether alerts triggered outside of this notification's <see cref="Schedule"/> should be sent out when the scheduled pause ends.
        /// </summary>
        [XmlElement("injected_postpone")]
        public bool? Postpone
        {
            get { return Lazy(() => postpone); }
            set { postpone = value; }
        }

        private string comments { get; set; }

        /// <summary>
        /// Comments present on this object.
        /// </summary>
        [XmlElement("injected_comments")]
        public string Comments
        {
            get { return Lazy(() => comments); }
            set { comments = value; }
        }

        private string scheduleStr { get; set; }

        [XmlElement("injected_schedule")]
        internal string lazyScheduleStr
        {
            get { return Lazy(() => scheduleStr); }
            set { scheduleStr = value; }
        }

        internal Lazy<Schedule> schedule;

        /// <summary>
        /// The schedule during which the notification action is active. When inactive, alerts sent to this notification will be ignored unless <see cref="Postpone"/> is specified.
        /// </summary>
        public Schedule Schedule => Lazy(() => schedule?.Value); //Set via UpdateTriggerActions, UpdateActionSchedules

        #endregion
        #region Notification Summarization

        private SummaryMode? summaryMode { get; set; }

        /// <summary>
        /// Specifies how PRTG should summarize notifications when multiple alerts occur within quick succession.
        /// </summary>
        [XmlElement("injected_summode")]
        public SummaryMode? SummaryMode
        {
            get { return Lazy(() => summaryMode); }
            set { summaryMode = value; }
        }

        private string summarySubject { get; set; }

        /// <summary>
        /// The subject line to use for summarize notification alerts.
        /// </summary>
        [XmlElement("injected_summarysubject")]
        public string SummarySubject
        {
            get { return Lazy(() => summarySubject); }
            set { summarySubject = value; }
        }
        
        private int? summaryPeriod { get; set; }

        /// <summary>
        /// The timespan (in seconds) during which PRTG will gather new notifications before sending a summarization alert.
        /// </summary>
        [XmlElement("injected_summinutes")]
        public int? SummaryPeriod
        {
            get { return Lazy(() => summaryPeriod); }
            set { summaryPeriod = value; }
        }

        #endregion
        #region Wrappers

        //todo: test that all of them are lazy, or otherwise test that all properties contain a call to the lazy method?
        //or otherwise try to access the property, and then confirm doing so triggers a call to the full api call. use the special
        //thing that specifies ready(2) or whatever for 2 more requests

        private NotificationActionEmailSettings email { get; set; }

        /// <summary>
        /// Settings that apply to Email Notification Actions.
        /// </summary>
        [XmlElement(CategoryEmail)]
        public NotificationActionEmailSettings Email
        {
            get { return Lazy(() => email); }
            set { email = value; }
        }

        private NotificationActionPushSettings push { get; set; }

        /// <summary>
        /// Settings that apply to Email Notification Actions.
        /// </summary>
        [XmlElement(CategoryPush)]
        public NotificationActionPushSettings Push
        {
            get { return Lazy(() => push); }
            set { push = value; }
        }
        
        private NotificationActionSMSSettings sms { get; set; }

        /// <summary>
        /// Settings that apply to SMS/Pager Notification Actions.
        /// </summary>
        [XmlElement(CategorySMS)]
        public NotificationActionSMSSettings SMS
        {
            get { return Lazy(() => sms); }
            set { sms = value; }
        }
        
        private NotificationActionEventLogSettings eventLog { get; set; }

        /// <summary>
        /// Settings that apply to Event Log Notification Actions.
        /// </summary>
        [XmlElement(CategoryEventLog)]
        public NotificationActionEventLogSettings EventLog
        {
            get { return Lazy(() => eventLog); }
            set { eventLog = value; }
        }

        private NotificationActionSyslogSettings syslog { get; set; }

        /// <summary>
        /// Settings that apply to Syslog Notification Actions.
        /// </summary>
        [XmlElement(CategorySyslog)]
        public NotificationActionSyslogSettings Syslog
        {
            get { return Lazy(() => syslog); }
            set { syslog = value; }
        }
        
        private NotificationActionSNMPSettings snmp { get; set; }

        /// <summary>
        /// Settings that apply to SNMP Notification Actions.
        /// </summary>
        [XmlElement(CategorySNMP)]
        public NotificationActionSNMPSettings SNMP
        {
            get { return Lazy(() => snmp); }
            set { snmp = value; }
        }
        
        private NotificationActionHttpSettings http { get; set; }

        /// <summary>
        /// Settings that apply to HTTP Notification Actions.
        /// </summary>
        [XmlElement(CategoryHttp)]
        public NotificationActionHttpSettings Http
        {
            get { return Lazy(() => http); }
            set { http = value; }
        }
        
        private NotificationActionProgramSettings program { get; set; }

        /// <summary>
        /// Setings that apply to Program Notification Actions.
        /// </summary>
        [XmlElement(CategoryProgram)]
        public NotificationActionProgramSettings Program
        {
            get { return Lazy(() => program); }
            set { program = value; }
        }
        
        private NotificationActionAmazonSettings amazon { get; set; }

        /// <summary>
        /// Settings that apply to Amazon Notification Actions.
        /// </summary>
        [XmlElement(CategoryAmazon)]
        public NotificationActionAmazonSettings Amazon
        {
            get { return Lazy(() => amazon); }
            set { amazon = value; }
        }

        private NotificationActionTicketSettings ticket { get; set; }

        /// <summary>
        /// Settings that apply to Ticket Notification Actions.
        /// </summary>
        [XmlElement(CategoryTicket)]
        public NotificationActionTicketSettings Ticket
        {
            get { return Lazy(() => ticket); }
            set { ticket = value; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAction"/> class.
        /// </summary>
        public NotificationAction()
        {
        }

        internal NotificationAction(string raw) : base(raw)
        {
        }

        string ISerializable.GetSerializedFormat()
        {
            return $"{Id}|{Name}";
        }

        #region ILazy

        Lazy<XDocument> ILazy.LazyXml { get; set; }

        [ExcludeFromCodeCoverage]
        internal Lazy<XDocument> LazyXml
        {
            get { return ((ILazy) this).LazyXml; }
            set { ((ILazy) this).LazyXml = value; }
        }

        object ILazy.LazyLock { get; } = new object();

        bool ILazy.LazyInitialized { get; set; }

        #endregion
    }
}