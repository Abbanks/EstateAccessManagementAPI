using EstateAccessManagement.Application.Interfaces.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EstateAccessManagement.Infrastructure.BackgroundJobs;

public class NotificationBackgroundService(IServiceProvider serviceProvider,
            IMessageQueueClient messageQueueClient,
            ILogger<NotificationBackgroundService> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("NotificationBackgroundService started.");

        messageQueueClient.SubscribeAsync("AccessCodeNotifications", async (message) =>
        {
            using var scope = serviceProvider.CreateScope();
            var worker = scope.ServiceProvider.GetRequiredService<NotificationWorker>();

            await worker.HandleMessageAsync(message);
        });

        logger.LogInformation("NotificationBackgroundService is stopping.");
        return Task.CompletedTask;
    }
}


