var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
                      .WithDataBindMount(source: @"./postgres-data", isReadOnly: false);
var postgresDb = postgres.AddDatabase("doctorsappointmentsdb");

var rabbit = builder.AddRabbitMQ("messaging");

var dataProject = builder.AddProject<Projects.DoctorsAppointments_Data>("doctorsappointments-data")
        .WithReference(postgresDb)
        .WaitFor(postgresDb)
        .WithArgs("--seedtestdata");

builder.AddProject<Projects.DoctorsAppointments_Api>("doctorsappointments-api")
        .WithReference(postgresDb)
        .WithReference(rabbit)
        .WaitForCompletion(dataProject);

builder.Build().Run();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);