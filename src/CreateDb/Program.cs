using Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MassTransit;
using Serilog;
using Serilog.Events;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var services = new ServiceCollection();

services.AddDbContext<RegistrationDbContext>(x =>
{
    var connectionString = config.GetConnectionString("Default");

    x.UseSqlServer(connectionString, options =>
    {
        options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        options.MigrationsHistoryTable($"__{nameof(RegistrationDbContext)}");

        options.MinBatchSize(1);
    });
});

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();

var logger = Log.Logger;
var dbContext = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();

logger.Information("Applying migrations for DbContext");

await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();

logger.Information("Migrations completed for DbContext");


