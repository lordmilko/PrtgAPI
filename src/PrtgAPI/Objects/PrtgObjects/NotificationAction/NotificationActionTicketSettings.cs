using System.Collections.Generic;
using System.Xml.Serialization;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Settings that apply to Ticket Notification Actions.
    /// </summary>
    [XmlRoot(NotificationAction.CategoryTicket)]
    public class NotificationActionTicketSettings : BaseNotificationActionSettings
    {
        /// <summary>
        /// If true, specifies that new tickets will be assigned to a <see cref="UserGroup"/>. If false, tickets will be assigned to a specific <see cref="UserAccount"/>.
        /// </summary>
        [XmlElement("injected_isusergroup")]
        public bool IsUserGroup { get; set; }

        [XmlElement("injected_ticketuserid")]
        internal string userAccountStr { get; set; }

        private LazyValue<UserAccount> userAccount;

        /// <summary>
        /// The PRTG User tickets will be assigned to. Applies only if <see cref="IsUserGroup"/> is false.
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
        /// The PRTG User Group tickets will be assigned to. Applies only if <see cref="IsUserGroup"/> is true.
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
        /// The subject to use for new tickets.
        /// </summary>
        [XmlElement("injected_subject")]
        public string Subject { get; set; }

        /// <summary>
        /// The message to include in the ticket.
        /// </summary>
        [XmlElement("injected_message")]
        public string Message { get; set; }

        /// <summary>
        /// Whether tickets should automatically close themselves when the condition that triggered the notification clears.
        /// </summary>
        [XmlElement("injected_autoclose")]
        public bool AutoClose { get; set; }

        internal override void ToString(List<object> targets)
        {
            if (IsSet(UserAccount))
                targets.Add(UserAccount);

            if (IsSet(UserGroup))
                targets.Add(UserGroup);
        }
    }
}