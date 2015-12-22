using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Notifications
{
    /// <summary>
    /// Notification manager for notifications
    /// </summary>
    /// <remarks>
    /// Where such notifications are displayed depends on the theme used. Default themes contain a 
    /// Messages zone for this.
    /// </remarks>
    public interface INotifier
    {
        /// <summary>
        /// Adds a new UI notification
        /// </summary>
        /// <param name="type">
        /// The type of the notification (notifications with different types can be displayed differently)</param>
        /// <param name="message">A localized message to display</param>
        void Add(NotifyType type, LocalizedString message);

        /// <summary>
        /// Get all notifications added
        /// </summary>
        IEnumerable<NotifyEntry> List();
    }
}
