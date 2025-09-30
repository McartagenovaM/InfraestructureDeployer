using System.ComponentModel.DataAnnotations;

namespace infrastrucure.web.Models;

public sealed class StatusPatchDto
{
    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;
}
