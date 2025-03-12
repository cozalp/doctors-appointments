using DoctorsAppointments.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsAppointments.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDoctorAppointmentsApplicationLayer(
            this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IAttendeeService, AttendeeService>();

            return services;
        }
    }
}