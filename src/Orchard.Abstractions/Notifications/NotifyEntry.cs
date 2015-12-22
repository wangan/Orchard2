using Microsoft.Extensions.Localization;

namespace Orchard.Notifications
{
    public enum NotifyType
    {
        Information,
        Warning,
        Error
    }

    public class NotifyEntry
    {
        public NotifyType Type { get; set; }
        public LocalizedString Message { get; set; }
    }
}
