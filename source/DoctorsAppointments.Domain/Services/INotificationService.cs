using DoctorsAppointments.Domain.Entities;

namespace DoctorsAppointments.Domain.Services
{
    /// <summary>
    /// Interface for notification services
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification about an event to an attendee
        /// </summary>
        /// <param name="event">The event information</param>
        /// <param name="attendee">The attendee to notify</param>
        /// <param name="notificationType">The type of notification to send</param>
        /// <returns>void</returns>
        Task SendNotificationAsync(Event @event, Attendee attendee, NotificationType notificationType);
    }

    /// <summary>
    /// Types of notifications that can be sent
    /// </summary>
    public enum NotificationType
    {
        Invitation,
        Reminder,
        Cancellation,
        Rescheduled,
        UpdatedDetails
    }
}