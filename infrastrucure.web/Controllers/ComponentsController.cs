using infrastrucure.web.Models;
using infrastrucure.web.Services;
using Microsoft.AspNetCore.Mvc;

namespace infrastrucure.web.Controllers;

public sealed class ComponentsController : Controller
{
    private static readonly string[] EnvironmentOptions = ["dev", "qa", "prod"];
    private static readonly string[] TypeOptions = ["vm", "appservice", "sql", "vnet"];
    private static readonly string[] StatusOptions = ["provisioned", "deploying", "failed", "deleted"];

    private readonly ComponentsApiClient _apiClient;

    public ComponentsController(ComponentsApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? environment, string? type, string? status)
    {
        var viewModel = new ComponentsIndexViewModel
        {
            Environment = environment,
            Type = type,
            Status = status,
            EnvironmentOptions = EnvironmentOptions,
            TypeOptions = TypeOptions,
            StatusOptions = StatusOptions
        };

        try
        {
            var components = await _apiClient.GetAllAsync(environment, type, status);
            viewModel.Components = components;
        }
        catch (HttpRequestException ex)
        {
            TempData["ToastMessage"] = $"Unable to load components. {ex.Message}";
            TempData["ToastType"] = "danger";
        }

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        PopulateSelectLists();
        return View(new ComponentCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ComponentCreateDto input)
    {
        if (!ModelState.IsValid)
        {
            PopulateSelectLists();
            return View(input);
        }

        try
        {
            await _apiClient.CreateAsync(input);
            SetToast("Component created successfully.", "success");
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            PopulateSelectLists();
            return View(input);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var component = await _apiClient.GetByIdAsync(id);
        if (component is null)
        {
            return NotFound();
        }

        PopulateSelectLists(includeStatus: true);

        var dto = new ComponentUpdateDto
        {
            Name = component.Name,
            Type = component.Type,
            Environment = component.Environment,
            Status = component.Status
        };

        ViewData["ComponentId"] = component.Id;
        ViewData["CreatedUtc"] = component.CreatedUtc;
        ViewData["UpdatedUtc"] = component.UpdatedUtc;

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ComponentUpdateDto input)
    {
        if (!ModelState.IsValid)
        {
            PopulateSelectLists(includeStatus: true);
            ViewData["ComponentId"] = id;
            return View(input);
        }

        try
        {
            await _apiClient.UpdateAsync(id, input);
            SetToast("Component updated successfully.", "success");
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            PopulateSelectLists(includeStatus: true);
            ViewData["ComponentId"] = id;
            return View(input);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _apiClient.DeleteAsync(id);
            SetToast(deleted ? "Component deleted successfully." : "Component not found.", deleted ? "success" : "warning");
        }
        catch (HttpRequestException ex)
        {
            SetToast($"Failed to delete component. {ex.Message}", "danger");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Provision(Guid id) => ExecuteQuickAction(id, _apiClient.ProvisionAsync, "Component provision triggered.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Deploy(Guid id) => ExecuteQuickAction(id, _apiClient.DeployAsync, "Component deployment started.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Teardown(Guid id) => ExecuteQuickAction(id, _apiClient.TeardownAsync, "Component teardown initiated.");

    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }

    private async Task<IActionResult> ExecuteQuickAction(Guid id, Func<Guid, CancellationToken, Task<ComponentReadDto>> action, string successMessage)
    {
        try
        {
            await action(id, HttpContext.RequestAborted);
            SetToast(successMessage, "success");
        }
        catch (HttpRequestException ex)
        {
            SetToast($"Action failed. {ex.Message}", "danger");
        }

        return RedirectToAction(nameof(Index));
    }

    private void PopulateSelectLists(bool includeStatus = false)
    {
        ViewData["EnvironmentOptions"] = EnvironmentOptions;
        ViewData["TypeOptions"] = TypeOptions;
        if (includeStatus)
        {
            ViewData["StatusOptions"] = StatusOptions;
        }
    }

    private void SetToast(string message, string type)
    {
        TempData["ToastMessage"] = message;
        TempData["ToastType"] = type;
    }
}
