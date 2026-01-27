using ServicesLibrary.Groups.Dtos;
using ServicesLibrary.Groups.Requests;
using SharedLibrary.DTOs;
using System.Net.Http.Json;

namespace WebInterface.Services;

public class UserApiService
{
    private readonly HttpClient _httpClient;

    public UserApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<UserDto>>("api/users") ?? new List<UserDto>();
        }
        catch
        {
            return new List<UserDto>();
        }
    }

    public async Task<UserDto?> GetUserAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task DeleteUserAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/users/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<GroupDto>> GetGroupsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<GroupDto>>("api/groups") ?? new List<GroupDto>();
        }
        catch
        {
            return new List<GroupDto>();
        }
    }

    public async Task<GroupDetailDto?> GetGroupDetailsAsync(int groupId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<GroupDetailDto>($"api/groups/{groupId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<PermissionDto>>("api/groups/permissions") 
                ?? new List<PermissionDto>();
        }
        catch
        {
            return new List<PermissionDto>();
        }
    }

    public async Task<List<PermissionDto>> GetAvailablePermissionsForGroupAsync(int groupId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<PermissionDto>>($"api/groups/{groupId}/available-permissions") 
                ?? new List<PermissionDto>();
        }
        catch
        {
            return new List<PermissionDto>();
        }
    }

    public async Task<GroupDetailDto?> AddPermissionToGroupAsync(int groupId, int permissionId)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/groups/{groupId}/permissions", 
            new AddPermissionToGroupRequest(permissionId));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GroupDetailDto>();
    }

    public async Task<GroupDetailDto?> RemovePermissionFromGroupAsync(int groupId, int permissionId)
    {
        var response = await _httpClient.DeleteAsync($"api/groups/{groupId}/permissions/{permissionId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GroupDetailDto>();
    }

    public async Task<int> GetTotalUserCountAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<int>("api/users/count");
            return result;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<int>("api/users/count/active");
            return result;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<Dictionary<string, int>> GetUserCountPerGroupAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Dictionary<string, int>>("api/users/count/per-group") 
                ?? new Dictionary<string, int>();
        }
        catch
        {
            return new Dictionary<string, int>();
        }
    }

    public async Task<UserStatisticsDto?> GetUserStatisticsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserStatisticsDto>("api/users/statistics");
        }
        catch
        {
            return null;
        }
    }
}
