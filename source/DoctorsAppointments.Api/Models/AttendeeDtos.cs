using DoctorsAppointments.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace DoctorsAppointments.Api.Models
{
    public class AttendeeRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        public AttendanceStatus Status { get; set; } = AttendanceStatus.NotResponded;
    }

    public class AttendeeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int EventId { get; set; }
    }

    public class AttendanceUpdateRequestDto
    {
        [Required]
        public AttendanceStatus Status { get; set; }
    }
}