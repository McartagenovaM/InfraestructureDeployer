using System.ComponentModel.DataAnnotations;

namespace infrastructure.api.Dtos;

public sealed class ComponentUpdateDto
{
    [Required]
    public string Name { get; set; } = default!;

    [Required]
    public string Type { get; set; } = default!;

    [Required]
    public string Environment { get; set; } = default!;

    [Required]
    public string Status { get; set; } = default!;

    public Dictionary<string, string>? Metadata { get; set; }
}
