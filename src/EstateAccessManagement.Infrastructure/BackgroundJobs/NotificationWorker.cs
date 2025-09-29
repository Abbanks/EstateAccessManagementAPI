using EstateAccessManagement.Application.Interfaces.Email;
using EstateAccessManagement.Application.Interfaces.Messaging;
using EstateAccessManagement.Application.Interfaces.Services;
using EstateAccessManagement.Core.AccessCodes;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EstateAccessManagement.Infrastructure.BackgroundJobs;

public class NotificationWorker(
    IMessageQueueClient messageQueueClient,
    IEmailService emailService,
    IUserService userService,
    ILogger<NotificationWorker> logger)
{
    public async Task HandleMessageAsync(string message)
    {
        var notification = JsonSerializer.Deserialize<AccessCodeNotification>(message);

        var resident = await userService.GetUserById(notification.ResidentId);
        if (resident == null || string.IsNullOrEmpty(resident.Email))
        {
            logger.LogWarning("Resident email is null or empty for ResidentId: {ResidentId}", notification.ResidentId);
            return;
        }

        var subject = "New Access Code Generated";
        var body = $"Hi {resident.FirstName},<br/>Your new access code is <b>{notification.AccessCode}</b>. It expires on {notification.ExpiresAt}.";

        await emailService.SendEmailAsync(notification.ResidentEmail, subject, body);
        logger.LogInformation("Notification sent to {Email}", resident.Email);
    }
}

