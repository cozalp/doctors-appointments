using System.ComponentModel.DataAnnotations;

namespace DoctorsAppointments.Domain.Entities
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public ICollection<Attendee> Attendees { get; set; } = new List<Attendee>();

        public bool IsValid()
        {
            return EndTime > StartTime;
        }

        public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
    }
}