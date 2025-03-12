using DoctorsAppointments.Domain.Entities;

namespace DoctorsAppointments.Domain.Repositories
{
    /// <summary>
    /// Repository interface for event-specific operations
    /// </summary>
    public interface IEventRepository : IRepository<Event>
    {
        Task<Event?> GetEventWithAttendeesAsync(int id);
        Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
        Task<bool> IsTimeSlotAvailableAsync(DateTime startTime, DateTime endTime, int? excludeEventId = null);
    }
}