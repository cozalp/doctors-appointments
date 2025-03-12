using DoctorsAppointments.Application;
using DoctorsAppointments.Data;
using DoctorsAppointments.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();

// Register application layers using extension methods
builder.Services.AddDoctorAppointmentsDataLayer(
    builder.Configuration.GetConnectionString("doctorsappointmentsdb") ?? "");
builder.Services.AddDoctorAppointmentsApplicationLayer();
builder.Services.AddDoctorAppointmentsInfrastructureLayer(builder.Configuration);

// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Doctor's Appointments API",
        Version = "v1",
        Description = "An API for managing doctor's appointments and scheduling",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@doctorsapp.com"
        }
    });
    
    // Use XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Doctor's Appointments API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app root
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
