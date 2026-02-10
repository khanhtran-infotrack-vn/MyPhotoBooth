namespace MyPhotoBooth.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string email, string resetToken, string callbackUrl, CancellationToken cancellationToken = default);
}
