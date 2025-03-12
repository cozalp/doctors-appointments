using DoctorsAppointments.Domain.Entities;
using DoctorsAppointments.Domain.Repositories;
using DoctorsAppointments.Domain.Services;

namespace DoctorsAppointments.Application.Services
{
    /// <summary>
    /// Implementation of the event management service
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IEventRepository eventRepository;
        private readonly INotificationService notificationService;

        public EventService(IEventRepository EventRepository, INotificationService NotificationService)
        {
            eventRepository = EventRepository;
            notificationService = NotificationService;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await eventRepository.GetAllAsync();
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await eventRepository.GetByIdAsync(id);
        }

        public async Task<Event?> GetEventWithAttendeesAsync(int id)
        {
            return await eventRepository.GetEventWithAttendeesAsync(id);
        }

        public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await eventRepository.GetEventsByDateRangeAsync(start, end);
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
        {
            return await eventRepository.SearchEventsAsync(searchTerm);
        }

        public async Task<Event> CreateEventAsync(Event eventEntity)
        {
            if (!eventEntity.IsValid())
            {
                throw new ArgumentException("Event is not valid. End time must be after start time.");
            }

            var isTimeSlotAvailable = await eventRepository.IsTimeSlotAvailableAsync(
                eventEntity.StartTime, eventEntity.EndTime);

            if (!isTimeSlotAvailable)
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }

            await eventRepository.AddAsync(eventEntity);
            await eventRepository.SaveChangesAsync();

            if (eventEntity.Attendees.Any())
            {
                foreach (var attendee in eventEntity.Attendees)
                {
                    await notificationService.SendNotificationAsync(
                        eventEntity, attendee, NotificationType.Invitation);
                }
            }

            return eventEntity;
        }

        public async Task<Event?> UpdateEventAsync(Event eventEntity)
        {
            if (!eventEntity.IsValid())
            {
                throw new ArgumentException("Event is not valid. End time must be after start time.");
            }

            var existingEvent = await eventRepository.GetEventWithAttendeesAsync(eventEntity.Id);
            if (existingEvent == null)
            {
                return null;
            }

            var isTimeSlotAvailable = await eventRepository.IsTimeSlotAvailableAsync(
                eventEntity.StartTime, eventEntity.EndTime, eventEntity.Id);

            if (!isTimeSlotAvailable)
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }

            bool timeChanged = existingEvent.StartTime != eventEntity.StartTime ||
                              existingEvent.EndTime != eventEntity.EndTime;

            bool detailsChanged = existingEvent.Title != eventEntity.Title ||
                                 existingEvent.Description != eventEntity.Description;

            existingEvent.Title = eventEntity.Title;
            existingEvent.Description = eventEntity.Description;
            existingEvent.StartTime = eventEntity.StartTime;
            existingEvent.EndTime = eventEntity.EndTime;

            eventRepository.Update(existingEvent);
            await eventRepository.SaveChangesAsync();

            if (existingEvent.Attendees.Any())
            {
                NotificationType notificationType = NotificationType.UpdatedDetails;

                if (timeChanged)
                {
                    notificationType = NotificationType.Rescheduled;
                }

                foreach (var attendee in existingEvent.Attendees)
                {
                    await notificationService.SendNotificationAsync(
                        existingEvent, attendee, notificationType);
                }
            }

            return existingEvent;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var eventToDelete = await eventRepository.GetEventWithAttendeesAsync(id);
            if (eventToDelete == null)
            {
                return false;
            }

            var attendees = new List<Attendee>(eventToDelete.Attendees);

            eventRepository.Remove(eventToDelete);
            await eventRepository.SaveChangesAsync();

            foreach (var attendee in attendees)
            {
                await notificationService.SendNotificationAsync(
                    eventToDelete, attendee, NotificationType.Cancellation);
            }

            return true;
        }

        public async Task<bool> IsTimeSlotAvailableAsync(DateTime startTime, DateTime endTime, int? excludeEventId = null)
        {
            return await eventRepository.IsTimeSlotAvailableAsync(startTime, endTime, excludeEventId);
        }
    }
}