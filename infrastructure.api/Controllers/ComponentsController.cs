using infrastructure.api.Data;
using infrastructure.api.Dtos;
using infrastructure.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.api.Controllers;

[ApiController]
[Route("api/components")]
public sealed class ComponentsController : ControllerBase
{
    private const string ProvisionedStatus = "provisioned";
    private const string DeployingStatus = "deploying";
    private const string DeletedStatus = "deleted";

    private static readonly HashSet<string> AllowedEnvironments = new(StringComparer.OrdinalIgnoreCase) { "dev", "qa", "prod" };
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase) { ProvisionedStatus, DeployingStatus, "failed", DeletedStatus };

    private readonly AppDbContext _context;

    public ComponentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComponentReadDto>>> GetComponents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? env = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        if (page < 1)
        {
            return CreateValidationProblem("page", "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            return CreateValidationProblem("pageSize", "Page size must be greater than or equal to 1.");
        }

        IQueryable<InfrastructureComponent> query = _context.Components.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(env))
        {
            if (!IsValidEnvironment(env))
            {
                return CreateValidationProblem(nameof(env), $"Environment must be one of: {string.Join(", ", AllowedEnvironments)}.");
            }

            var normalizedEnv = Normalize(env);
            query = query.Where(c => c.Environment == normalizedEnv);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(c => c.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!IsValidStatus(status))
            {
                return CreateValidationProblem(nameof(status), $"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
            }

            var normalizedStatus = Normalize(status);
            query = query.Where(c => c.Status == normalizedStatus);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers["X-Total-Count"] = totalCount.ToString();

        return Ok(items.Select(ToReadDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ComponentReadDto>> GetComponent(Guid id)
    {
        var component = await _context.Components.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        if (component is null)
        {
            return NotFound();
        }

        return Ok(ToReadDto(component));
    }

    [HttpPost]
    public async Task<ActionResult<ComponentReadDto>> CreateComponent(ComponentCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!IsValidEnvironment(dto.Environment))
        {
            return CreateValidationProblem(nameof(dto.Environment), $"Environment must be one of: {string.Join(", ", AllowedEnvironments)}.");
        }

        var component = new InfrastructureComponent
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Type = dto.Type,
            Environment = Normalize(dto.Environment),
            Status = ProvisionedStatus,
            CreatedUtc = DateTime.UtcNow,
            Metadata = dto.Metadata is null ? null : new Dictionary<string, string>(dto.Metadata)
        };

        _context.Components.Add(component);
        await _context.SaveChangesAsync();

        var readDto = ToReadDto(component);
        return CreatedAtAction(nameof(GetComponent), new { id = component.Id }, readDto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ComponentReadDto>> UpdateComponent(Guid id, ComponentUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!IsValidEnvironment(dto.Environment))
        {
            return CreateValidationProblem(nameof(dto.Environment), $"Environment must be one of: {string.Join(", ", AllowedEnvironments)}.");
        }

        if (!IsValidStatus(dto.Status))
        {
            return CreateValidationProblem(nameof(dto.Status), $"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
        }

        var component = await _context.Components.FirstOrDefaultAsync(c => c.Id == id);

        if (component is null)
        {
            return NotFound();
        }

        component.Name = dto.Name;
        component.Type = dto.Type;
        component.Environment = Normalize(dto.Environment);
        component.Status = Normalize(dto.Status);
        component.Metadata = dto.Metadata is null ? null : new Dictionary<string, string>(dto.Metadata);
        component.UpdatedUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ToReadDto(component));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ComponentReadDto>> PatchStatus(Guid id, StatusPatchDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!IsValidStatus(dto.Status))
        {
            return CreateValidationProblem(nameof(dto.Status), $"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
        }

        var component = await _context.Components.FirstOrDefaultAsync(c => c.Id == id);

        if (component is null)
        {
            return NotFound();
        }

        component.Status = Normalize(dto.Status);
        component.UpdatedUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ToReadDto(component));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComponent(Guid id)
    {
        var component = await _context.Components.FirstOrDefaultAsync(c => c.Id == id);

        if (component is null)
        {
            return NotFound();
        }

        if (!string.Equals(component.Status, DeletedStatus, StringComparison.Ordinal))
        {
            component.Status = DeletedStatus;
            component.UpdatedUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        _context.Components.Remove(component);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/provision")]
    public async Task<ActionResult<ComponentReadDto>> Provision(Guid id)
    {
        var component = await _context.Components.FirstOrDefaultAsync(c => c.Id == id);

        if (component is null)
        {
            return NotFound();
        }

        await TransitionAsync(component, DeployingStatus, ProvisionedStatus);

        return Ok(ToReadDto(component));
    }

    [HttpPost("{id:guid}/deploy")]
    public async Task<ActionResult<ComponentReadDto>> Deploy(Guid id)
    {
        var component = await _context.Components.FirstOrDefaultAsync(c => c.Id == id);

        if (component is null)
        {
            return NotFound();
        }

        if (!string.Equals(component.Status, ProvisionedStatus, StringComparison.Ordinal))
        {
            return CreateValidationProblem("status", "Only provisioned components can be deployed.");
        }

        await TransitionAsync(component, DeployingStatus, ProvisionedStatus);

        return Ok(ToReadDto(component));
    }

    [HttpPost("{id:guid}/teardown")]
    public async Task<ActionResult<ComponentReadDto>> Teardown(Guid id)
    {
        var component = await _context.Components.FirstOrDefaultAsync(c => c.Id == id);

        if (component is null)
        {
            return NotFound();
        }

        component.Status = DeletedStatus;
        component.UpdatedUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ToReadDto(component));
    }

    private async Task TransitionAsync(InfrastructureComponent component, string interimStatus, string finalStatus)
    {
        component.Status = interimStatus;
        component.UpdatedUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await Task.Delay(150);

        component.Status = finalStatus;
        component.UpdatedUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static ComponentReadDto ToReadDto(InfrastructureComponent component)
    {
        return new ComponentReadDto
        {
            Id = component.Id,
            Name = component.Name,
            Type = component.Type,
            Environment = component.Environment,
            Status = component.Status,
            CreatedUtc = component.CreatedUtc,
            UpdatedUtc = component.UpdatedUtc,
            Metadata = component.Metadata is null ? null : new Dictionary<string, string>(component.Metadata)
        };
    }

    private ActionResult CreateValidationProblem(string key, string message)
    {
        ModelState.AddModelError(key, message);
        return ValidationProblem(ModelState);
    }

    private static bool IsValidEnvironment(string value) => AllowedEnvironments.Contains(value);

    private static bool IsValidStatus(string value) => AllowedStatuses.Contains(value);

    private static string Normalize(string value) => value.ToLowerInvariant();
}
