using EstateAccessManagement.Application.Interfaces.Messaging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EstateAccessManagement.Infrastructure.Messaging
{
    public class RabbitMqClient : IMessageQueueClient, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _publishChannel;
        private readonly ILogger<RabbitMqClient> _logger;

        private RabbitMqClient(IConnection connection, IChannel publishChannel, ILogger<RabbitMqClient> logger)
        {
            _connection = connection;
            _publishChannel = publishChannel;
            _logger = logger;
        }

        public static async Task<RabbitMqClient> CreateAsync(
            string hostName,
            int port,
            string userName,
            string password,
            string vhost,
            ILogger<RabbitMqClient> logger)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                VirtualHost = vhost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                ClientProvidedName = "EstateAccessManagement"
            };

            var connection = await factory.CreateConnectionAsync();
            var publishChannel = await connection.CreateChannelAsync();

            logger.LogInformation("RabbitMQ async connection established to {Host}:{Port}/{VHost}", hostName, port, vhost);

            return new RabbitMqClient(connection, publishChannel, logger);
        }

        public async Task PublishAsync(string queueName, string message)
        {
            await _publishChannel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties
            {
                DeliveryMode = (DeliveryModes)2,
                ContentType = "text/plain"
            };

            await _publishChannel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: true,
                basicProperties: props,
                body: body);

            _logger.LogInformation("Message published to queue {QueueName}", queueName);
        }

        public async Task SubscribeAsync(string queueName, Func<string, Task> onMessage)
        {
            var consumerChannel = await _connection.CreateChannelAsync();

            await consumerChannel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(consumerChannel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                try
                {
                    await onMessage(message);
                    await consumerChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Message acknowledged from {QueueName}", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message processing failed for {QueueName}", queueName);
                    await consumerChannel.BasicRejectAsync(ea.DeliveryTag, requeue: true);
                }
            };

            await consumerChannel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);

            _logger.LogInformation("Subscribed to queue {QueueName}", queueName);
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            if (_publishChannel?.IsOpen == true)
            {
                await _publishChannel.CloseAsync();
                await _publishChannel.DisposeAsync();
            }

            if (_connection?.IsOpen == true)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            _logger.LogInformation("RabbitMQ async connection closed.");
        }
    }
}
