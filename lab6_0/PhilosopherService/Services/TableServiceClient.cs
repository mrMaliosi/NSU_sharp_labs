using System.Net.Http.Json;
using PhilosopherService.Models;

namespace PhilosopherService.Services;

public class TableServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TableServiceClient> _logger;
    private readonly string _baseUrl;

    public TableServiceClient(HttpClient httpClient, ILogger<TableServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = Environment.GetEnvironmentVariable("TABLE_SERVICE_URL") ?? "http://table-service:8080";
    }

    public async Task<bool> RegisterPhilosopherAsync(string philosopherId, string philosopherName)
    {
        try
        {
            var request = new RegisterPhilosopherRequest
            {
                PhilosopherId = philosopherId,
                PhilosopherName = philosopherName
            };
            
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/philosophers/register", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации философа");
            return false;
        }
    }

    public async Task<bool> AcquireForkAsync(int forkId, string philosopherId, string philosopherName)
    {
        try
        {
            var request = new AcquireForkRequest
            {
                PhilosopherId = philosopherId,
                PhilosopherName = philosopherName
            };
            
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/forks/{forkId}/acquire", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AcquireForkResponse>();
                return result?.Success ?? false;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при захвате вилки {ForkId}", forkId);
            return false;
        }
    }

    public async Task<bool> ReleaseForkAsync(int forkId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/forks/{forkId}/release", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при освобождении вилки {ForkId}", forkId);
            return false;
        }
    }

    public async Task<bool> RecordMealAsync(string philosopherName)
    {
        try
        {
            var request = new RecordMealRequest
            {
                PhilosopherName = philosopherName
            };
            
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/metrics/meal", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при записи приема пищи");
            return false;
        }
    }

    public async Task<bool> UpdateWaitingTimeAsync(string philosopherName, long waitingTimeMs)
    {
        try
        {
            var request = new UpdateWaitingTimeRequest
            {
                PhilosopherName = philosopherName,
                WaitingTimeMs = waitingTimeMs
            };
            
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/metrics/waiting-time", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении времени ожидания");
            return false;
        }
    }

    public async Task<bool> PhilosopherExitAsync(string philosopherId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/philosophers/{philosopherId}/exit", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выходе философа");
            return false;
        }
    }
}

