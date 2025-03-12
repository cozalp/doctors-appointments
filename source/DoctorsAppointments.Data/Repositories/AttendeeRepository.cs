using DoctorsAppointments.Domain.Entities;
using DoctorsAppointments.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DoctorsAppointments.Data.Repositories
{
    public class AttendeeRepository : Repository<Attendee>, IAttendeeRepository
    {
        public AttendeeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Attendee>> GetAttendeesByEventIdAsync(int eventId)
        {
            return await _context.Attendees
                .Where(a => a.EventId == eventId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByAttendeeEmailAsync(string email)
        {
            return await _context.Attendees
                .Where(a => a.Email == email)
                .Include(a => a.Event)
                .Select(a => a.Event!)
                .ToListAsync();
        }

        public async Task<bool> UpdateAttendanceStatusAsync(int attendeeId, AttendanceStatus status)
        {
            var attendee = await _context.Attendees.FindAsync(attendeeId);
            if (attendee == null)
                return false;

            attendee.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}