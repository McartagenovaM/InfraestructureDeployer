using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using infrastrucure.web.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace infrastrucure.web.Services;

public sealed class ComponentsApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ComponentsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<ComponentReadDto>> GetAllAsync(string? environment = null, string? type = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var queryParameters = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(environment))
        {
            queryParameters["env"] = environment;
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            queryParameters["type"] = type;
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            queryParameters["status"] = status;
        }

        var endpoint = "api/components";
        if (queryParameters.Count > 0)
        {
            endpoint = QueryHelpers.AddQueryString(endpoint, queryParameters!);
        }

        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        await EnsureSuccess(response);

        var components = await response.Content.ReadFromJsonAsync<List<ComponentReadDto>>(JsonOptions, cancellationToken);
        return components ?? new List<ComponentReadDto>();
    }

    public async Task<ComponentReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/components/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<ComponentReadDto>(JsonOptions, cancellationToken);
    }

    public async Task<ComponentReadDto> CreateAsync(ComponentCreateDto dto, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/components", dto, JsonOptions, cancellationToken);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<ComponentReadDto>(JsonOptions, cancellationToken))!;
    }

    public async Task<ComponentReadDto> UpdateAsync(Guid id, ComponentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"api/components/{id}", dto, JsonOptions, cancellationToken);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<ComponentReadDto>(JsonOptions, cancellationToken))!;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"api/components/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccess(response);
        return true;
    }

    public async Task<ComponentReadDto> ProvisionAsync(Guid id, CancellationToken cancellationToken = default) => await PostActionAsync(id, "provision", cancellationToken);

    public async Task<ComponentReadDto> DeployAsync(Guid id, CancellationToken cancellationToken = default) => await PostActionAsync(id, "deploy", cancellationToken);

    public async Task<ComponentReadDto> TeardownAsync(Guid id, CancellationToken cancellationToken = default) => await PostActionAsync(id, "teardown", cancellationToken);

    public async Task<ComponentReadDto> PatchStatusAsync(Guid id, StatusPatchDto dto, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/components/{id}/status")
        {
            Content = JsonContent.Create(dto, options: JsonOptions)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<ComponentReadDto>(JsonOptions, cancellationToken))!;
    }

    private async Task<ComponentReadDto> PostActionAsync(Guid id, string action, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync($"api/components/{id}/{action}", null, cancellationToken);
        await EnsureSuccess(response);
        return (await response.Content.ReadFromJsonAsync<ComponentReadDto>(JsonOptions, cancellationToken))!;
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API request failed with status {(int)response.StatusCode}: {response.ReasonPhrase}. {error}");
        }
    }
}
