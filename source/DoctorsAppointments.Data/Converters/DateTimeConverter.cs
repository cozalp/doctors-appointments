using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DoctorsAppointments.Data.Converters
{
    public class DateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public DateTimeConverter()
            : base(
                d => d.ToUniversalTime(),
                d => d.ToUniversalTime())
        {
        }
    }
}