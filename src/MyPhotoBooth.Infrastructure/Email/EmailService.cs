using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ITemplateEngine _templateEngine;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _provider;
    private readonly string? _mailpitUrl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IHttpClientFactory httpClientFactory, ITemplateEngine templateEngine)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _templateEngine = templateEngine;
        _provider = _configuration["EmailSettings:Provider"] ?? "Mailpit";
        _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@photobooth.local";
        _fromName = _configuration["EmailSettings:FromName"] ?? "MyPhotoBooth";
        _mailpitUrl = _configuration["EmailSettings:MailpitUrl"];
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null, CancellationToken cancellationToken = default)
    {
        if (_provider.Equals("Mailpit", StringComparison.OrdinalIgnoreCase))
        {
            await SendViaMailpitAsync(toEmail, subject, htmlContent, plainTextContent, cancellationToken);
        }
        else if (_provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
        {
            await SendViaSendGridAsync(toEmail, subject, htmlContent, plainTextContent, cancellationToken);
        }
        else
        {
            _logger.LogWarning("Unknown email provider: {Provider}. Email not sent.", _provider);
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, string callbackUrl, CancellationToken cancellationToken = default)
    {
        var template = await _templateEngine.GetTemplateAsync("PasswordReset", cancellationToken);

        var variables = new Dictionary<string, string>
        {
            { "appName", _fromName },
            { "resetLink", callbackUrl },
            { "expiryTime", "2 hours" },
            { "year", DateTime.UtcNow.Year.ToString() }
        };

        var htmlContent = await _templateEngine.RenderTemplateAsync("PasswordReset", variables, cancellationToken);
        var textContent = await _templateEngine.RenderTemplateAsync("PasswordReset", variables, cancellationToken);

        await SendEmailAsync(email, template.Subject, htmlContent, textContent, cancellationToken);
    }

    private async Task SendViaMailpitAsync(string toEmail, string subject, string htmlContent, string? plainTextContent, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_mailpitUrl))
        {
            _logger.LogWarning("Mailpit URL not configured. Email would be sent to: {ToEmail}", toEmail);
            _logger.LogInformation("Email Subject: {Subject}", subject);
            _logger.LogInformation("Email Content (HTML): {HtmlContent}", htmlContent);
            return;
        }

        try
        {
            var mailpitApiUrl = $"{_mailpitUrl}/api/v1/send";

            // Mailpit API format: https://github.com/axllent/mailpit/wiki/HTTP-Send-API
            var payload = new
            {
                @from = new
                {
                    name = _fromName,
                    email = _fromEmail
                },
                to = new[]
                {
                    new
                    {
                        email = toEmail
                    }
                },
                subject = subject,
                html = htmlContent,
                text = plainTextContent ?? string.Empty
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();

            var response = await _httpClient.PostAsync(mailpitApiUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully via Mailpit to: {ToEmail}", toEmail);
            }
            else
            {
                _logger.LogWarning("Failed to send email via Mailpit. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via Mailpit to: {ToEmail}", toEmail);
            throw;
        }
    }

    private async Task SendViaSendGridAsync(string toEmail, string subject, string htmlContent, string? plainTextContent, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["EmailSettings:SendGridApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("SendGrid API key not configured");
            return;
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                personalizations = new[]
                {
                    new
                    {
                        to = new[] { new { email = toEmail } },
                        subject = subject
                    }
                },
                from = new { email = _fromEmail, name = _fromName },
                content = new[]
                {
                    new
                    {
                        type = "text/html",
                        value = htmlContent
                    },
                    !string.IsNullOrEmpty(plainTextContent) ? new
                    {
                        type = "text/plain",
                        value = plainTextContent
                    } : null
                }.Where(c => c != null).ToArray()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.sendgrid.com/v3/mail/send", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully via SendGrid to: {ToEmail}", toEmail);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to send email via SendGrid. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via SendGrid to: {ToEmail}", toEmail);
            throw;
        }
    }
}
