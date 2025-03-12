using System.ComponentModel.DataAnnotations;

namespace DoctorsAppointments.Domain.Entities
{
    public class Attendee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        public AttendanceStatus Status { get; set; } = AttendanceStatus.NotResponded;

        public int EventId { get; set; }

        public Event? Event { get; set; }
    }

    public enum AttendanceStatus
    {
        NotResponded = 0,
        Accepted = 1,
        Declined = 2,
        Tentative = 3
    }
}