using System;
using System.ComponentModel;
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
    [Description("Notification Action")]
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

        [XmlElement("injected_postpone")]
        internal bool? postpone { get; set; }

        /// <summary>
        /// Whether alerts triggered outside of this notification's <see cref="Schedule"/> should be sent out when the scheduled pause ends.
        /// </summary>
        public bool? Postpone => Lazy(() => postpone);

        [XmlElement("injected_comments")]
        internal string comments { get; set; }

        /// <summary>
        /// Comments present on this object.
        /// </summary>
        public string Comments => Lazy(() => comments);

        [XmlElement("injected_schedule")]
        internal string scheduleStr { get; set; }

        internal string lazyScheduleStr => Lazy(() => scheduleStr);

        internal Lazy<Schedule> schedule;

        /// <summary>
        /// The schedule during which the notification action is active. When inactive, alerts sent to this notification will be ignored unless <see cref="Postpone"/> is specified.
        /// </summary>
        public Schedule Schedule => Lazy(() => schedule?.Value); //Set via UpdateTriggerActions, UpdateActionSchedules

        #endregion
        #region Notification Summarization

        [XmlElement("injected_summode")]
        internal SummaryMode? summaryMode { get; set; }

        /// <summary>
        /// Specifies how PRTG should summarize notifications when multiple alerts occur within quick succession.
        /// </summary>
        public SummaryMode? SummaryMode => Lazy(() => summaryMode);

        [XmlElement("injected_summarysubject")]
        internal string summarySubject { get; set; }

        /// <summary>
        /// The subject line to use for summarize notification alerts.
        /// </summary>
        public string SummarySubject => Lazy(() => summarySubject);

        [XmlElement("injected_summinutes")]
        internal int? summaryPeriod { get; set; }

        /// <summary>
        /// The timespan (in seconds) during which PRTG will gather new notifications before sending a summarization alert.
        /// </summary>
        public int? SummaryPeriod => Lazy(() => summaryPeriod);

        #endregion
        #region Wrappers

        /// <summary>
        /// Settings that apply to Email Notification Actions.
        /// </summary>
        [XmlElement(CategoryEmail)]
        public NotificationActionEmailSettings Email { get; set; }

        /// <summary>
        /// Settings that apply to Email Notification Actions.
        /// </summary>
        [XmlElement(CategoryPush)]
        public NotificationActionPushSettings Push { get; set; }

        /// <summary>
        /// Settings that apply to SMS/Pager Notification Actions.
        /// </summary>
        [XmlElement(CategorySMS)]
        public NotificationActionSMSSettings SMS { get; set; }

        /// <summary>
        /// Settings that apply to Event Log Notification Actions.
        /// </summary>
        [XmlElement(CategoryEventLog)]
        public NotificationActionEventLogSettings EventLog { get; set; }

        /// <summary>
        /// Settings that apply to Syslog Notification Actions.
        /// </summary>
        [XmlElement(CategorySyslog)]
        public NotificationActionSyslogSettings Syslog { get; set; }

        /// <summary>
        /// Settings that apply to SNMP Notification Actions.
        /// </summary>
        [XmlElement(CategorySNMP)]
        public NotificationActionSNMPSettings SNMP { get; set; }

        /// <summary>
        /// Settings that apply to HTTP Notification Actions.
        /// </summary>
        [XmlElement(CategoryHttp)]
        public NotificationActionHttpSettings Http { get; set; }

        /// <summary>
        /// Setings that apply to Program Notification Actions.
        /// </summary>
        [XmlElement(CategoryProgram)]
        public NotificationActionProgramSettings Program { get; set; }

        /// <summary>
        /// Settings that apply to Amazon Notification Actions.
        /// </summary>
        [XmlElement(CategoryAmazon)]
        public NotificationActionAmazonSettings Amazon { get; set; }

        /// <summary>
        /// Settings that apply to Ticket Notification Actions.
        /// </summary>
        [XmlElement(CategoryTicket)]
        public NotificationActionTicketSettings Ticket { get; set; }

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
            get { return ((ILazy)this).LazyXml;}
            set { ((ILazy)this).LazyXml = value; }
        }

        object ILazy.LazyLock { get; } = new object();

        bool ILazy.LazyInitialized { get; set; }

        #endregion
    }
}