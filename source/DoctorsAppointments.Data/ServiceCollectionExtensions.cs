using DoctorsAppointments.Data.Repositories;
using DoctorsAppointments.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorsAppointments.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDoctorAppointmentsDataLayer(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IAttendeeRepository, AttendeeRepository>();

            return services;
        }
    }
}