using DoctorsAppointments.Domain.Entities;

namespace DoctorsAppointments.Domain.Repositories
{

    /// <summary>
    /// Repository interface for attendee-specific operations
    /// </summary>
    public interface IAttendeeRepository : IRepository<Attendee>
    {
        Task<IEnumerable<Attendee>> GetAttendeesByEventIdAsync(int eventId);
        Task<IEnumerable<Event>> GetEventsByAttendeeEmailAsync(string email);
        Task<bool> UpdateAttendanceStatusAsync(int attendeeId, AttendanceStatus status);
    }
}