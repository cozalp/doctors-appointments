using DoctorsAppointments.Domain.Entities;
using DoctorsAppointments.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace DoctorsAppointments.Infrastructure.Notifications
{
    public class EmailNotificationService : INotificationService
    {
        private readonly ILogger<EmailNotificationService> logger;
        private readonly EmailSettings emailSettings;

        public EmailNotificationService(ILogger<EmailNotificationService> Logger, EmailSettings EmailSettings)
        {
            logger = Logger;
            emailSettings = EmailSettings;
        }

        public async Task SendNotificationAsync(Event @event, Attendee attendee, NotificationType notificationType)
        {
            string subject = GetSubject(@event, notificationType);
            string body = GetEmailBody(@event, attendee, notificationType);

            try
            {
                logger.LogInformation($"Email notification sent: TO: {attendee.Email}, SUBJECT: {subject}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to send notification to {attendee.Email} about event {subject}");
                throw;
            }
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            using var mail = new MailMessage();
            mail.From = new MailAddress(emailSettings.FromEmail, emailSettings.FromName);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            using var client = new SmtpClient(emailSettings.SmtpServer, emailSettings.SmtpPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(emailSettings.Username, emailSettings.Password);
            client.EnableSsl = emailSettings.EnableSsl;

            await client.SendMailAsync(mail);
        }

        private string GetSubject(Event @event, NotificationType notificationType)
        {
            return notificationType switch
            {
                NotificationType.Invitation => $"Invitation: {@event.Title}",
                NotificationType.Reminder => $"Reminder: {@event.Title} is upcoming",
                NotificationType.Cancellation => $"Cancelled: {@event.Title}",
                NotificationType.Rescheduled => $"Rescheduled: {@event.Title}",
                NotificationType.UpdatedDetails => $"Updated: {@event.Title}",
                _ => $"Event Notification: {@event.Title}"
            };
        }

        private string GetEmailBody(Event @event, Attendee attendee, NotificationType notificationType)
        {
            var startTime = @event.StartTime.ToString("dddd, MMMM d, yyyy 'at' h:mm tt");
            var endTime = @event.EndTime.ToString("h:mm tt");

            string greeting = $"Hello {attendee.Name},<br><br>";
            string eventDetails = $"<b>Event:</b> {@event.Title}<br>" +
                                 $"<b>When:</b> {startTime} to {endTime}<br>" +
                                 $"<b>Description:</b> {@event.Description}<br><br>";

            string mainContent = notificationType switch
            {
                NotificationType.Invitation =>
                    "You have been invited to the above event. Please confirm your attendance.",

                NotificationType.Reminder =>
                    "This is a friendly reminder about the upcoming event.",

                NotificationType.Cancellation =>
                    "We regret to inform you that this event has been cancelled.",

                NotificationType.Rescheduled =>
                    "This event has been rescheduled. Please review the updated time.",

                NotificationType.UpdatedDetails =>
                    "The details for this event have been updated.",

                _ => "This is a notification regarding your event."
            };

            string footer = "<br><br>Thank you,<br>Doctor's Appointment System";

            return greeting + eventDetails + mainContent + footer;
        }
    }

    public class EmailSettings
    {
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "Doctors Appointment System";
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
    }
}