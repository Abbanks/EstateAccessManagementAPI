using EstateAccessManagement.Application.Interfaces.Messaging;
using Microsoft.Extensions.Hosting;

namespace EstateAccessManagement.Infrastructure.Messaging
{
    public class RabbitMqInitializer : IHostedService
    {
        private readonly IMessageQueueClient _client;

        public RabbitMqInitializer(IMessageQueueClient client)
        {
            _client = client;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_client is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }
}
