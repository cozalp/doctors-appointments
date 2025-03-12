using DoctorsAppointments.Domain.Entities;
using DoctorsAppointments.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DoctorsAppointments.Data.Repositories
{

    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Attendees)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Events
                .Include(e => e.Attendees)
                .Where(e => (e.StartTime >= start && e.StartTime <= end) ||
                           (e.EndTime >= start && e.EndTime <= end) ||
                           (e.StartTime <= start && e.EndTime >= end))
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<Event?> GetEventWithAttendeesAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _context.Events
                .Where(e => e.Title.Contains(searchTerm) ||
                           (e.Description != null && e.Description.Contains(searchTerm)))
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<bool> IsTimeSlotAvailableAsync(DateTime startTime, DateTime endTime, int? excludeEventId = null)
        {
            var query = _context.Events
                .Where(e => (e.StartTime < endTime && e.EndTime > startTime));

            if (excludeEventId.HasValue)
            {
                query = query.Where(e => e.Id != excludeEventId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}