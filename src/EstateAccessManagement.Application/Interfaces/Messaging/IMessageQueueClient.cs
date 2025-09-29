namespace EstateAccessManagement.Application.Interfaces.Messaging
{
    public interface IMessageQueueClient
    {
        Task PublishAsync(string queueName, string message);
        Task SubscribeAsync(string queueName, Func<string, Task> onMessage);
    }
}
