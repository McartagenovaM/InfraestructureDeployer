using System.ComponentModel.DataAnnotations;

namespace infrastructure.api.Dtos;

public sealed class StatusPatchDto
{
    [Required]
    public string Status { get; set; } = default!;
}
