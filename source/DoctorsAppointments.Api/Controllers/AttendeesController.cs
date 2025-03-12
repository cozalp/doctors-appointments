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
    public class AttendeesController : ControllerBase
    {
        private readonly IAttendeeService _attendeeService;
        private readonly IEventService _eventService;
        private readonly ILogger<AttendeesController> _logger;

        public AttendeesController(
            IAttendeeService attendeeService,
            IEventService eventService,
            ILogger<AttendeesController> logger)
        {
            _attendeeService = attendeeService;
            _eventService = eventService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all attendees for a specific event
        /// </summary>
        /// <param name="eventId">The ID of the event</param>
        /// <returns>A list of attendees</returns>
        /// <response code="200">Returns the list of attendees</response>
        /// <response code="404">If the event was not found</response>
        [HttpGet("event/{eventId}")]
        [ProducesResponseType(typeof(IEnumerable<AttendeeResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttendeesByEvent(int eventId)
        {
            try
            {
                // Check if event exists
                var eventEntity = await _eventService.GetEventByIdAsync(eventId);
                if (eventEntity == null)
                {
                    return NotFound($"Event with ID {eventId} not found");
                }

                var attendees = await _attendeeService.GetAttendeesByEventIdAsync(eventId);
                var attendeeDtos = attendees.Select(MapToAttendeeResponseDto).ToList();

                return Ok(attendeeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving attendees for event {eventId}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets a specific attendee by ID
        /// </summary>
        /// <param name="id">The ID of the attendee</param>
        /// <returns>The attendee data</returns>
        /// <response code="200">Returns the attendee</response>
        /// <response code="404">If the attendee was not found</response>
        [HttpGet("{id}", Name = "GetAttendeeById")]
        [ProducesResponseType(typeof(AttendeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttendee(int id)
        {
            try
            {
                var attendee = await _attendeeService.GetAttendeeByIdAsync(id);
                if (attendee == null)
                {
                    return NotFound();
                }

                return Ok(MapToAttendeeResponseDto(attendee));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving attendee with ID {id}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets all events for a specific attendee by email
        /// </summary>
        /// <param name="email">The email address of the attendee</param>
        /// <returns>A list of events the attendee is part of</returns>
        /// <response code="200">Returns the list of events</response>
        [HttpGet("email/{email}/events")]
        [ProducesResponseType(typeof(IEnumerable<EventResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventsByAttendeeEmail(string email)
        {
            try
            {
                var events = await _attendeeService.GetEventsByAttendeeEmailAsync(email);
                var eventDtos = events.Select(e => new EventResponseDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    DurationMinutes = e.DurationMinutes,
                    Attendees = e.Attendees.Select(MapToAttendeeResponseDto).ToList()
                }).ToList();

                return Ok(eventDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving events for attendee {email}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Adds an attendee to an event
        /// </summary>
        /// <param name="eventId">The ID of the event</param>
        /// <param name="attendeeDto">The attendee data</param>
        /// <returns>The created attendee</returns>
        /// <response code="201">Returns the newly created attendee</response>
        /// <response code="400">If the attendee data is invalid</response>
        /// <response code="404">If the event was not found</response>
        [HttpPost("event/{eventId}")]
        [ProducesResponseType(typeof(AttendeeResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAttendeeToEvent(int eventId, AttendeeRequestDto attendeeDto)
        {
            try
            {
                // Check if event exists
                var eventEntity = await _eventService.GetEventByIdAsync(eventId);
                if (eventEntity == null)
                {
                    return NotFound($"Event with ID {eventId} not found");
                }

                // Create and add the attendee
                var attendee = new Attendee
                {
                    EventId = eventId,
                    Name = attendeeDto.Name,
                    Email = attendeeDto.Email,
                    Status = attendeeDto.Status
                };

                var createdAttendee = await _attendeeService.AddAttendeeToEventAsync(attendee);
                var responseDto = MapToAttendeeResponseDto(createdAttendee);

                return CreatedAtRoute("GetAttendeeById", new { id = responseDto.Id }, responseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while adding attendee to event {eventId}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Updates an attendee's attendance status
        /// </summary>
        /// <param name="id">The ID of the attendee</param>
        /// <param name="updateDto">The attendance update data</param>
        /// <returns>No content</returns>
        /// <response code="204">If the attendee was successfully updated</response>
        /// <response code="404">If the attendee was not found</response>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAttendanceStatus(int id, AttendanceUpdateRequestDto updateDto)
        {
            try
            {
                var result = await _attendeeService.UpdateAttendanceStatusAsync(id, updateDto.Status);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating attendance status for attendee {id}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Removes an attendee from an event
        /// </summary>
        /// <param name="id">The ID of the attendee</param>
        /// <returns>No content</returns>
        /// <response code="204">If the attendee was successfully removed</response>
        /// <response code="404">If the attendee was not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveAttendeeFromEvent(int id)
        {
            try
            {
                var result = await _attendeeService.RemoveAttendeeFromEventAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while removing attendee {id}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Maps a domain attendee entity to a response DTO
        /// </summary>
        private static AttendeeResponseDto MapToAttendeeResponseDto(Attendee attendee)
        {
            return new AttendeeResponseDto
            {
                Id = attendee.Id,
                Name = attendee.Name,
                Email = attendee.Email,
                Status = attendee.Status.ToString(),
                EventId = attendee.EventId
            };
        }
    }
}