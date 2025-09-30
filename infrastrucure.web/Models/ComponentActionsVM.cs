using System;

namespace infrastrucure.web.Models;

public sealed class ComponentActionsVM
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public bool CanProvision { get; set; }
    public bool CanDeploy { get; set; }
    public bool CanTeardown { get; set; }
    public bool CanDelete { get; set; }
}
