namespace infrastructure.api.Models;

public sealed class InfrastructureComponent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;        // vm, appservice, sql, vnet
    public string Environment { get; set; } = default!; // dev, qa, prod
    public string Status { get; set; } = "provisioned"; // provisioned, deploying, failed, deleted
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
