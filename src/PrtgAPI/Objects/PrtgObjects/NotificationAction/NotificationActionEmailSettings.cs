using System.Collections.Generic;
using System.Xml.Serialization;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Settings that apply to Email Notification Actions.
    /// </summary>
    [XmlRoot(NotificationAction.CategoryEmail)]
    public class NotificationActionEmailSettings : BaseNotificationActionSettings
    {
        [XmlElement("injected_addressuserid")]
        internal string userAccountStr { get; set; }

        private LazyValue<UserAccount> userAccount;

        /// <summary>
        /// PRTG user to send notification to. Can be used in conjunction with <see cref="UserGroup"/> and <see cref="Address"/>.
        /// </summary>
        public UserAccount UserAccount
        {
            get
            {
                if (userAccount == null)
                    userAccount = new LazyValue<UserAccount>(userAccountStr, () => new UserAccount(userAccountStr));

                return userAccount.Value;
            }
        }

        [XmlElement("injected_addressgroupid")]
        internal string userGroupStr { get; set; }

        private LazyValue<UserGroup> userGroup;

        /// <summary>
        /// PRTG group to send notification to. Can be used in conjunction with <see cref="UserAccount"/> and <see cref="Address"/>.
        /// </summary>
        public UserGroup UserGroup
        {
            get
            {
                if (userGroup == null)
                    userGroup = new LazyValue<UserGroup>(userGroupStr, () => new UserGroup(userGroupStr));

                return userGroup.Value;
            }
        }

        /// <summary>
        /// Email address to send notification to. Can be used in cojunction with <see cref="UserAccount"/> and <see cref="UserGroup"/>.
        /// </summary>
        [XmlElement("injected_address")]
        public string Address { get; set; }

        /// <summary>
        /// Subject to use for the email.
        /// </summary>
        [XmlElement("injected_subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Content type to use for the email.
        /// </summary>
        [XmlElement("injected_contenttype")]
        public EmailContentType ContentType { get; set; }

        /// <summary>
        /// Text to display in email body when <see cref="ContentType"/> is <see cref="EmailContentType.Custom"/>.
        /// </summary>
        [XmlElement("injected_customtext")]
        public string CustomText { get; set; }

        /// <summary>
        /// The priority to use for the email.
        /// </summary>
        [XmlElement("injected_priority")]
        public EmailPriority Priority { get; set; }

        internal override void ToString(List<object> targets)
        {
            if (IsSet(UserAccount))
                targets.Add(UserAccount);

            if (IsSet(UserGroup))
                targets.Add(UserGroup);

            if (Address != null)
                targets.Add(Address);
        }
    }
}