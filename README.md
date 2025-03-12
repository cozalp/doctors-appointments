# Doctors Appointments

A doctor's appointment management API built with .NET 9 and PostgreSQL; using layered architecture following clean architecture principles.

## Architecture Overview

This solution follows a clean architecture pattern with the following layers:

```
                    ┌───────────────────┐
                    │     API Layer     │
                    │  (Presentation)   │
                    └─────────┬─────────┘
                              │
                              ▼
                    ┌───────────────────┐
                    │  Application      │
                    │  Layer            │
                    └─────────┬─────────┘
                              │
           ┌─────────────────┬─────────────────┐
           │                 │                 │
┌──────────▼───────┐ ┌───────▼────────┐ ┌──────▼───────────┐
│  Domain Layer    │ │  Data Layer    │ │  Infrastructure  │
│                  │ │                │ │  Layer           │
└──────────────────┘ └────────────────┘ └──────────────────┘
```

## Project Structure

The solution is organized into several projects, each with a specific responsibility:

### DoctorsAppointments.Api

Web API project that serves as the entry point for HTTP requests. It contains:

- Controllers 
- API models (DTOs)
- Request/Response formatting
- API documentation

### DoctorsAppointments.Application

Contains the application's business logic and orchestrates the flow of data between the API and Domain layers:

- Services
- Command/Query handlers
- Application-specific interfaces

### DoctorsAppointments.Domain

Contains the core business entities and logic:

- Entity definitions
- Domain events
- Domain services
- Repository interfaces

### DoctorsAppointments.Data

Handles data persistence and implements repository interfaces:

- Entity Framework Core DbContext
- Repository implementations
- Migrations
- Data seeding logic

### DoctorsAppointments.Infrastructure

Provides implementation for external services and cross-cutting concerns:

- Email services
- Notification services
- File storage
- External API integrations

### DoctorsAppointments.ServiceDefaults

Contains shared service configurations and defaults for the entire application:

- Logging defaults
- Authentication/Authorization defaults
- Common middleware configurations

### DoctorsAppointments.AppHost

Container orchestration project using .NET Aspire:

- Service discovery
- Container configurations
- PostgreSQL and RabbitMQ resource definitions

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Visual Studio 2022
- Aspire workload for Visual Studio 2022

## Running the Application

### Using Visual Studio 2022

1. Open the solution file `DoctorsAppointments.sln` in Visual Studio
2. Set `DoctorsAppointments.AppHost` as the startup project
3. Press F5 or click the "Start" button to run the application
4. The .NET Aspire dashboard will open automatically, showing you all services and their health
5. Wait for doctorsappointments-api to get to the running state, then click the one of its endpoints to open the Swagger UI

### Using Command Line

1. Navigate to the solution root directory
2. Run the application using the following commands:

```bash
cd DoctorsAppointments
dotnet build
cd DoctorsAppointments.AppHost
dotnet run
```

3. Access the API at: `https://localhost:7146/index.html` (port may vary)
4. Access the .NET Aspire dashboard at: `https://localhost:17093` (port may vary)

## Database Setup

The application uses PostgreSQL, which is automatically provisioned via .NET Aspire when running the AppHost project. However, if you want to run with a standalone database:

1. Update the connection string in `DoctorsAppointments.Api/appsettings.json`
2. Run the migrations:

```bash
cd DoctorsAppointments.Data
dotnet ef database update
```

## Seeding Test Data

The application includes a test data seeder that populates the database with sample data. This happens automatically on application startup in development mode.

## API Documentation

API documentation is available via Swagger UI when running the application. Navigate to `/swagger` endpoint to explore the available endpoints.