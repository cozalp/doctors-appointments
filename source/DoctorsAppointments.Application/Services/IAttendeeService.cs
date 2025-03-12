using DoctorsAppointments.Domain.Entities;

namespace DoctorsAppointments.Application.Services
{
    /// <summary>
    /// Interface for attendee management services
    /// </summary>
    public interface IAttendeeService
    {
        Task<IEnumerable<Attendee>> GetAllAttendeesAsync();
        Task<Attendee?> GetAttendeeByIdAsync(int id);
        Task<IEnumerable<Attendee>> GetAttendeesByEventIdAsync(int eventId);
        Task<IEnumerable<Event>> GetEventsByAttendeeEmailAsync(string email);
        Task<Attendee> AddAttendeeToEventAsync(Attendee attendee);
        Task<bool> RemoveAttendeeFromEventAsync(int id);
        Task<bool> UpdateAttendanceStatusAsync(int attendeeId, AttendanceStatus status);
    }
}