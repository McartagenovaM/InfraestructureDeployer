namespace infrastructure.api.Dtos;

public sealed class ComponentReadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Environment { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
