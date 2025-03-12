using DoctorsAppointments.Api.Models;
using DoctorsAppointments.Application.Services;
using DoctorsAppointments.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace DoctorsAppointments.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventService eventService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of all events with optional filtering
        /// </summary>
        /// <param name="parameters">Search and paging parameters</param>
        /// <returns>A list of events</returns>
        /// <response code="200">Returns the list of events</response>
        [HttpGet]
        [ProducesResponseType(typeof(PageResult<EventResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvents([FromQuery] EventSearchQueryParameters parameters)
        {
            try
            {
                IEnumerable<Event> events;

                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                {
                    events = await _eventService.SearchEventsAsync(parameters.SearchTerm);
                }
                else if (parameters.StartDate.HasValue && parameters.EndDate.HasValue)
                {
                    events = await _eventService.GetEventsByDateRangeAsync(parameters.StartDate.Value, parameters.EndDate.Value);
                }
                else
                {
                    events = await _eventService.GetAllEventsAsync();
                }

                // Apply pagination
                var totalCount = events.Count();
                var pagedEvents = events
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToList();

                var eventDtos = pagedEvents.Select(MapToEventResponseDto).ToList();

                return Ok(new PageResult<EventResponseDto>(
                    eventDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets a specific event by ID
        /// </summary>
        /// <param name="id">The ID of the event</param>
        /// <returns>The event data</returns>
        /// <response code="200">Returns the event</response>
        /// <response code="404">If the event was not found</response>
        [HttpGet("{id}", Name = "GetEventById")]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvent(int id)
        {
            try
            {
                var eventEntity = await _eventService.GetEventWithAttendeesAsync(id);

                if (eventEntity == null)
                {
                    return NotFound();
                }

                return Ok(MapToEventResponseDto(eventEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving event with ID {id}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <param name="eventDto">The event data</param>
        /// <returns>The created event</returns>
        /// <response code="201">Returns the newly created event</response>
        /// <response code="400">If the event data is invalid</response>
        [HttpPost]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEvent(EventRequestDto eventDto)
        {
            try
            {
                // Validate input
                if (eventDto.EndTime <= eventDto.StartTime)
                {
                    return BadRequest("End time must be after start time.");
                }

                // Check if time slot is available
                var isTimeSlotAvailable = await _eventService.IsTimeSlotAvailableAsync(
                    eventDto.StartTime, eventDto.EndTime);

                if (!isTimeSlotAvailable)
                {
                    return BadRequest("The selected time slot is not available.");
                }

                // Map DTO to domain entity
                var eventEntity = new Event
                {
                    Title = eventDto.Title,
                    Description = eventDto.Description,
                    StartTime = eventDto.StartTime,
                    EndTime = eventDto.EndTime
                };

                // Add attendees if provided
                if (eventDto.Attendees != null && eventDto.Attendees.Any())
                {
                    eventEntity.Attendees = eventDto.Attendees.Select(a => new Attendee
                    {
                        Name = a.Name,
                        Email = a.Email,
                        Status = a.Status
                    }).ToList();
                }

                var createdEvent = await _eventService.CreateEventAsync(eventEntity);

                var responseDto = MapToEventResponseDto(createdEvent);

                return CreatedAtRoute("GetEventById", new { id = responseDto.Id }, responseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating an event");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Updates an existing event
        /// </summary>
        /// <param name="id">The ID of the event to update</param>
        /// <param name="eventDto">The updated event data</param>
        /// <returns>No content</returns>
        /// <response code="204">If the event was successfully updated</response>
        /// <response code="400">If the event data is invalid</response>
        /// <response code="404">If the event was not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, EventRequestDto eventDto)
        {
            try
            {
                // Validate input
                if (eventDto.EndTime <= eventDto.StartTime)
                {
                    return BadRequest("End time must be after start time.");
                }

                var existingEvent = await _eventService.GetEventByIdAsync(id);
                if (existingEvent == null)
                {
                    return NotFound();
                }

                // Check if time slot is available (excluding this event)
                var isTimeSlotAvailable = await _eventService.IsTimeSlotAvailableAsync(
                    eventDto.StartTime, eventDto.EndTime, id);

                if (!isTimeSlotAvailable)
                {
                    return BadRequest("The selected time slot is not available.");
                }

                // Update event properties
                existingEvent.Title = eventDto.Title;
                existingEvent.Description = eventDto.Description;
                existingEvent.StartTime = eventDto.StartTime;
                existingEvent.EndTime = eventDto.EndTime;

                await _eventService.UpdateEventAsync(existingEvent);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating event with ID {id}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Deletes an event
        /// </summary>
        /// <param name="id">The ID of the event to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the event was successfully deleted</response>
        /// <response code="404">If the event was not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var result = await _eventService.DeleteEventAsync(id);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting event with ID {id}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Checks if a time slot is available
        /// </summary>
        /// <param name="startTime">The start time</param>
        /// <param name="endTime">The end time</param>
        /// <param name="excludeEventId">Optional event ID to exclude from the check</param>
        /// <returns>Whether the time slot is available</returns>
        [HttpGet("availability")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckAvailability(
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] int? excludeEventId = null)
        {
            try
            {
                if (endTime <= startTime)
                {
                    return BadRequest("End time must be after start time.");
                }

                var isAvailable = await _eventService.IsTimeSlotAvailableAsync(
                    startTime, endTime, excludeEventId);

                return Ok(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking time slot availability");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Maps a domain event entity to a response DTO
        /// </summary>
        private static EventResponseDto MapToEventResponseDto(Event eventEntity)
        {
            return new EventResponseDto
            {
                Id = eventEntity.Id,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                DurationMinutes = eventEntity.DurationMinutes,
                Attendees = eventEntity.Attendees
                    .Select(a => new AttendeeResponseDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Email = a.Email,
                        Status = a.Status.ToString(),
                        EventId = a.EventId
                    })
                    .ToList()
            };
        }
    }
}