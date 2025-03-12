using DoctorsAppointments.Domain.Services;
using DoctorsAppointments.Infrastructure.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsAppointments.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDoctorAppointmentsInfrastructureLayer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton(provider => new EmailSettings
            {
                FromEmail = configuration["EmailSettings:FromEmail"] ?? "noreply@doctorsapp.com",
                FromName = configuration["EmailSettings:FromName"] ?? "Doctors Appointment System",
                SmtpServer = configuration["EmailSettings:SmtpServer"] ?? "localhost",
                SmtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "25"),
                Username = configuration["EmailSettings:Username"] ?? "",
                Password = configuration["EmailSettings:Password"] ?? "",
                EnableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"] ?? "false")
            });

            services.AddScoped<INotificationService, EmailNotificationService>();

            return services;
        }
    }
}