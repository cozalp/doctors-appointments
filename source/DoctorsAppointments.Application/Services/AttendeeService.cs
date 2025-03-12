using DoctorsAppointments.Domain.Entities;
using DoctorsAppointments.Domain.Repositories;
using DoctorsAppointments.Domain.Services;

namespace DoctorsAppointments.Application.Services
{

    /// <summary>
    /// Implementation of the attendee management service
    /// </summary>
    public class AttendeeService : IAttendeeService
    {
        private readonly IAttendeeRepository _attendeeRepository;
        private readonly IEventRepository _eventRepository;
        private readonly INotificationService _notificationService;

        public AttendeeService(
            IAttendeeRepository attendeeRepository,
            IEventRepository eventRepository,
            INotificationService notificationService)
        {
            _attendeeRepository = attendeeRepository;
            _eventRepository = eventRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Attendee>> GetAllAttendeesAsync()
        {
            return await _attendeeRepository.GetAllAsync();
        }

        public async Task<Attendee?> GetAttendeeByIdAsync(int id)
        {
            return await _attendeeRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Attendee>> GetAttendeesByEventIdAsync(int eventId)
        {
            return await _attendeeRepository.GetAttendeesByEventIdAsync(eventId);
        }

        public async Task<IEnumerable<Event>> GetEventsByAttendeeEmailAsync(string email)
        {
            return await _attendeeRepository.GetEventsByAttendeeEmailAsync(email);
        }

        public async Task<Attendee> AddAttendeeToEventAsync(Attendee attendee)
        {
            // Validate the event exists
            var eventEntity = await _eventRepository.GetByIdAsync(attendee.EventId);
            if (eventEntity == null)
            {
                throw new ArgumentException($"Event with ID {attendee.EventId} does not exist.");
            }

            // Add the attendee
            await _attendeeRepository.AddAsync(attendee);
            await _attendeeRepository.SaveChangesAsync();

            // Send invitation notification
            await _notificationService.SendNotificationAsync(eventEntity, attendee, NotificationType.Invitation);

            return attendee;
        }

        public async Task<bool> RemoveAttendeeFromEventAsync(int id)
        {
            var attendee = await _attendeeRepository.GetByIdAsync(id);
            if (attendee == null)
            {
                return false;
            }

            // Get the event before removing the attendee for notification purposes
            var eventEntity = await _eventRepository.GetByIdAsync(attendee.EventId);

            _attendeeRepository.Remove(attendee);
            await _attendeeRepository.SaveChangesAsync();

            // Only send notification if event still exists
            if (eventEntity != null)
            {
                await _notificationService.SendNotificationAsync(eventEntity, attendee, NotificationType.Cancellation);
            }

            return true;
        }

        public async Task<bool> UpdateAttendanceStatusAsync(int attendeeId, AttendanceStatus status)
        {
            return await _attendeeRepository.UpdateAttendanceStatusAsync(attendeeId, status);
        }
    }
}