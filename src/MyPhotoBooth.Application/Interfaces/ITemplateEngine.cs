namespace MyPhotoBooth.Application.Interfaces;

public interface ITemplateEngine
{
    Task<string> RenderTemplateAsync(string templateName, object model, CancellationToken cancellationToken = default);
    Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> variables, CancellationToken cancellationToken = default);
    Task<EmailTemplate> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default);
}

public class EmailTemplate
{
    public required string Name { get; set; }
    public required string Subject { get; set; }
    public required string HtmlContent { get; set; }
    public string? TextContent { get; set; }
}
