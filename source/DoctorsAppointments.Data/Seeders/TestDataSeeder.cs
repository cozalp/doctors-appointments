using Bogus;
using DoctorsAppointments.Domain.Entities;
using System.Globalization;

namespace DoctorsAppointments.Data.Seeders
{
    public class TestDataSeeder
    {
        private readonly AppDbContext dbContext;

        public TestDataSeeder(AppDbContext DbContext)
        {
            dbContext = DbContext;
        }

        public async Task SeedTestDataAsync()
        {
            await SeedTestEventsAsync();
            await SeedTestAttendeesAsync();
        }

        private async Task SeedTestAttendeesAsync()
        {
            if (dbContext.Attendees.Any())
            {
                return;
            }

            var faker = new Faker("en");
            Random random = new Random();
            
            var events = dbContext.Events.ToList();
            if (!events.Any())
            {
                return;
            }

            var attendees = new List<Attendee>();

            foreach (var eventEntity in events)
            {
                int attendeeCount = random.Next(1, 6);
                
                for (int i = 0; i < attendeeCount; i++)
                {
                    var status = (AttendanceStatus)random.Next(0, 4); // 0=NotResponded, 1=Accepted, 2=Declined, 3=Tentative
                    
                    var attendee = new Attendee
                    {
                        Name = faker.Name.FullName(),
                        Email = faker.Internet.Email(),
                        Status = status,
                        EventId = eventEntity.Id
                    };
                    
                    attendees.Add(attendee);
                }
            }
            
            await dbContext.Attendees.AddRangeAsync(attendees);
            await dbContext.SaveChangesAsync();
        }

        private async Task SeedTestEventsAsync()
        {
            if (dbContext.Events.Any())
            {
                return;
            }

            var faker = new Faker("en");
            Random random = new Random();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            var appointmentTypes = new[]
            {
                "Checkup", "Follow-up", "Consultation", "Examination", "Screening",
                "Vaccination", "Treatment", "Therapy", "Surgery", "Emergency"
            };

            var doctorSpecialties = new[]
            {
                "General Practitioner", "Pediatrician", "Cardiologist", "Dermatologist", 
                "Neurologist", "Orthopedist", "Gynecologist", "Ophthalmologist", 
                "Dentist", "Psychiatrist", "ENT Specialist"
            };

            var events = new List<Event>();

            for (int i = 0; i < 50; i++)
            {
                var startTime = DateTime.Now.AddDays(random.Next(1, 30))
                    .AddHours(random.Next(8, 17))
                    .AddMinutes(random.Next(0, 4) * 15);
                
                int durationMinutes = (random.Next(1, 5) * 15);
                var endTime = startTime.AddMinutes(durationMinutes);
                
                var appointmentType = appointmentTypes[random.Next(appointmentTypes.Length)];
                var specialty = doctorSpecialties[random.Next(doctorSpecialties.Length)];
                var title = $"{appointmentType} with {specialty}";

                string description = faker.Lorem.Paragraph();
                
                var eventEntity = new Event
                {
                    Title = title,
                    Description = description,
                    StartTime = startTime,
                    EndTime = endTime
                };
                
                events.Add(eventEntity);
            }
            
            await dbContext.Events.AddRangeAsync(events);
            await dbContext.SaveChangesAsync();
        }
    }
}