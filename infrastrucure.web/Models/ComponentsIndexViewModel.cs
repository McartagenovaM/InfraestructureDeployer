using System;

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
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasResults => Components.Count > 0;
}
