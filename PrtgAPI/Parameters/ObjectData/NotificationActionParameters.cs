using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class NotificationActionParameters : ContentParameters<NotificationAction>
    {
        public NotificationActionParameters() : base(Content.Notifications)
        {
        }
    }
}
