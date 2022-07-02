namespace Producer;

using Components;
using Components.Contracts;
using MassTransit;
using System.Security.Cryptography;

public class SubmitRegistrationWorker : BackgroundService
{
    private readonly ILogger<SubmitRegistrationWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SubmitRegistrationWorker(
        ILogger<SubmitRegistrationWorker> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Submitting registration at: {time}", DateTimeOffset.Now);

            var scope = _serviceScopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            try
            {
                var registration = new Registration
                {
                    RegistrationId = NewId.NextGuid(),
                    RegistrationDate = DateTime.UtcNow,
                    MemberId = Guid.NewGuid().ToString(),
                    EventId = Guid.NewGuid().ToString(),
                    Payment = RandomNumberGenerator.GetInt32(1, 1000)
                };

                await dbContext.AddAsync(registration, stoppingToken);

                await publishEndpoint.Publish(new RegistrationSubmitted
                {
                    RegistrationId = registration.RegistrationId,
                    RegistrationDate = registration.RegistrationDate,
                    MemberId = registration.MemberId,
                    EventId = registration.EventId,
                    Payment = registration.Payment,
                }, stoppingToken);

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            finally
            {
                if (scope is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                else
                    scope.Dispose();
            }

            await Task.Delay(RandomNumberGenerator.GetInt32(500, 1000), stoppingToken);
        }
    }
}
