using EstateAccessManagement.Application.Interfaces.Email;
using EstateAccessManagement.Core.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EstateAccessManagement.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Attempted to send email but recipient address was null or empty. Subject: {Subject}", subject);
            return;
        }

        if (!MailboxAddress.TryParse(toEmail, out var mailboxAddress))
        {
            _logger.LogWarning("Attempted to send email but recipient address is invalid: {Email}. Subject: {Subject}", toEmail, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        message.To.Add(mailboxAddress);
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlContent
        };
        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);

            if (!string.IsNullOrWhiteSpace(_emailSettings.SmtpUser))
            {
                await smtpClient.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
            }

            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email} with subject {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending email to {Email}", toEmail);
            throw;
        }
    }
}

