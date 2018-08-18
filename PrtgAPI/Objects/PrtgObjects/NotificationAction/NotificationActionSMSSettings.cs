using System.Collections.Generic;
using System.Xml.Serialization;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Settings that apply to SMS/Pager Notification Actions.
    /// </summary>
    [XmlRoot(NotificationAction.CategorySMS)]
    public class NotificationActionSMSSettings : BaseNotificationActionSettings
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
        /// Message to include in the SMS/Pager alert.
        /// </summary>
        [XmlElement("injected_message")]
        public string Message { get; set; }

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