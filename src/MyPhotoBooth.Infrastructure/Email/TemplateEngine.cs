using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Infrastructure.Email;

public class TemplateEngine : ITemplateEngine
{
    private readonly ILogger<TemplateEngine> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _templatesPath;
    private readonly ConcurrentDictionary<string, string> _templateCache;

    public TemplateEngine(ILogger<TemplateEngine> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _templatesPath = _configuration["EmailSettings:TemplatesPath"]
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email", "Templates");
        _templateCache = new ConcurrentDictionary<string, string>();
    }

    public async Task<EmailTemplate> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default)
    {
        var metadataPath = Path.Combine(_templatesPath, $"{templateName}.metadata.json");
        var htmlPath = Path.Combine(_templatesPath, $"{templateName}.html");
        var textPath = Path.Combine(_templatesPath, $"{templateName}.txt");

        if (!File.Exists(metadataPath))
        {
            throw new FileNotFoundException($"Template metadata file not found: {metadataPath}");
        }

        if (!File.Exists(htmlPath))
        {
            throw new FileNotFoundException($"Template HTML file not found: {htmlPath}");
        }

        var metadata = await File.ReadAllTextAsync(metadataPath, cancellationToken);
        var htmlContent = await File.ReadAllTextAsync(htmlPath, cancellationToken);
        var textContent = File.Exists(textPath)
            ? await File.ReadAllTextAsync(textPath, cancellationToken)
            : null;

        var metadataDoc = System.Text.Json.JsonDocument.Parse(metadata);
        var subject = metadataDoc.RootElement.GetProperty("subject").GetString() ?? templateName;

        return new EmailTemplate
        {
            Name = templateName,
            Subject = subject,
            HtmlContent = htmlContent,
            TextContent = textContent
        };
    }

    public async Task<string> RenderTemplateAsync(string templateName, object model, CancellationToken cancellationToken = default)
    {
        var variables = ConvertModelToDictionary(model);
        return await RenderTemplateAsync(templateName, variables, cancellationToken);
    }

    public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        var templatePath = Path.Combine(_templatesPath, $"{templateName}.html");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }

        var template = _templateCache.GetOrAdd(templateName, _ =>
        {
            _logger.LogDebug("Loading template: {TemplateName}", templateName);
            return File.ReadAllText(templatePath);
        });

        var rendered = template;

        foreach (var kvp in variables)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            rendered = rendered.Replace(placeholder, kvp.Value);
        }

        return rendered;
    }

    private Dictionary<string, string> ConvertModelToDictionary(object model)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var properties = model.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(model)?.ToString() ?? string.Empty;
            dict[prop.Name] = value;
        }

        return dict;
    }
}
