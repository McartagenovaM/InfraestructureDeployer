namespace infrastrucure.web.Models;

public sealed class ComponentsIndexViewModel
{
    public IReadOnlyCollection<ComponentReadDto> Components { get; set; } = Array.Empty<ComponentReadDto>();
    public string? Environment { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public IReadOnlyCollection<string> EnvironmentOptions { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> TypeOptions { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> StatusOptions { get; set; } = Array.Empty<string>();
}
