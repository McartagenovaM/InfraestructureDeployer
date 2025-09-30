using System.ComponentModel.DataAnnotations;

namespace infrastrucure.web.Models;

public sealed class ComponentUpdateDto
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Type")]
    public string Type { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Environment")]
    public string Environment { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    public Dictionary<string, string>? Metadata { get; set; }
}
