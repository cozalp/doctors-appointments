using DoctorsAppointments.Domain.Entities;

namespace DoctorsAppointments.Application.Services
{
    /// <summary>
    /// Interface for event management services
    /// </summary>
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event?> GetEventByIdAsync(int id);
        Task<Event?> GetEventWithAttendeesAsync(int id);
        Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
        Task<Event> CreateEventAsync(Event eventEntity);
        Task<Event?> UpdateEventAsync(Event eventEntity);
        Task<bool> DeleteEventAsync(int id);
        Task<bool> IsTimeSlotAvailableAsync(DateTime startTime, DateTime endTime, int? excludeEventId = null);
    }
}