using DoctorsAppointments.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using DoctorsAppointments.Data.Seeders;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

string? connectionString = "";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configHost =>
    {
        configHost.SetBasePath(Directory.GetCurrentDirectory());
        configHost.AddJsonFile("datalayer.appsettings.json", optional: true);
        configHost.AddUserSecrets<Program>();
        configHost.AddEnvironmentVariables();
        configHost.AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        connectionString = hostContext.Configuration.GetConnectionString("doctorsappointmentsdb");

        services.AddDoctorAppointmentsDataLayer(connectionString ?? "");
        services.AddTransient<TestDataSeeder>();
        services.AddLogging();
    })
    .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console())
    .Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configurationService = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var testDataSeeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

        await dbContext.Database.MigrateAsync();

        logger.Information("Database connection successful.");

        if (args.Contains("--seedtestdata"))
        {
            await testDataSeeder.SeedTestDataAsync();

            logger.Information("Test data seeded.");
        }
        
        Environment.Exit(0);
    }
    catch (Exception ex)
    {
        Log.Fatal($"Database connection error, exiting.");
        Log.Fatal($"Exception : {ex.Message}");
        await host.StopAsync();
        Environment.Exit(0);
    }
}

await host.RunAsync();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);