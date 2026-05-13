using System.Net.Http.Json;

namespace PhilosopherService.Services;

public class PhilosopherWorker
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PhilosopherWorker> _logger;
    private readonly string _philosopherId;
    private readonly string _philosopherName;
    private readonly int _leftForkId;
    private readonly int _rightForkId;
    private readonly string _tableServiceUrl;
    private readonly int _simulationDurationMinutes;
    private readonly Random _random = new();

    private int _mealsEaten = 0;
    private int _totalThinkingTime = 0;
    private int _totalEatingTime = 0;

    public PhilosopherWorker(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PhilosopherWorker> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _logger = logger;

        _philosopherId = configuration["PHILOSOPHER_ID"] ?? throw new InvalidOperationException("PHILOSOPHER_ID not set");
        _philosopherName = configuration["PHILOSOPHER_NAME"] ?? throw new InvalidOperationException("PHILOSOPHER_NAME not set");
        _leftForkId = int.Parse(configuration["LEFT_FORK_ID"] ?? throw new InvalidOperationException("LEFT_FORK_ID not set"));
        _rightForkId = int.Parse(configuration["RIGHT_FORK_ID"] ?? throw new InvalidOperationException("RIGHT_FORK_ID not set"));
        _tableServiceUrl = configuration["TABLE_SERVICE_URL"] ?? throw new InvalidOperationException("TABLE_SERVICE_URL not set");
        _simulationDurationMinutes = int.Parse(configuration["SIMULATION_DURATION_MINUTES"] ?? "5");
    }

    public async Task StartSimulationAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"{_philosopherName} ({_philosopherId}) starting simulation for {_simulationDurationMinutes} minutes");

            // Register with table service
            await RegisterWithTableAsync();

            var endTime = DateTime.UtcNow.AddMinutes(_simulationDurationMinutes);

            while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
            {
                // Think
                await ThinkAsync();

                // Try to eat
                await TryEatAsync();

                // Small delay to prevent tight loop
                //await Task.Delay(100, cancellationToken);
            }

            _logger.LogInformation($"{_philosopherName} ({_philosopherId}) simulation ended. Meals eaten: {_mealsEaten}");

            // Send final stats and exit
            await UpdateStatsAsync();
            await ExitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in philosopher simulation for {_philosopherName}");
        }
    }

    private async Task RegisterWithTableAsync()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_tableServiceUrl}/api/table/register",
                new { PhilosopherId = _philosopherId, PhilosopherName = _philosopherName }
            );

            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"{_philosopherName} registered with table service");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to register {_philosopherName} with table service");
            throw;
        }
    }

    private async Task ThinkAsync()
    {
        var thinkingTime = _random.Next(30, 100);
        _totalThinkingTime += thinkingTime;
        _logger.LogDebug($"{_philosopherName} is thinking for {thinkingTime}ms");
        await Task.Delay(thinkingTime);
    }

    private async Task TryEatAsync()
    {
        _logger.LogInformation($"{_philosopherName} attempting to take left fork {_leftForkId}");

        // Try to take left fork
        var leftForkTaken = await TakeForkAsync(_leftForkId);
        if (!leftForkTaken)
        {
            _logger.LogDebug($"{_philosopherName} failed to take left fork {_leftForkId}");
            return;
        }

        _logger.LogInformation($"{_philosopherName} took left fork {_leftForkId}, attempting to take right fork {_rightForkId}");

        // Try to take right fork
        var rightForkTaken = await TakeForkAsync(_rightForkId);
        if (!rightForkTaken)
        {
            _logger.LogWarning($"{_philosopherName} failed to take right fork {_rightForkId}, releasing left fork");
            await ReleaseForkAsync(_leftForkId);
            return;
        }

        // Both forks acquired - eat
        _logger.LogInformation($"{_philosopherName} has both forks! Eating...");
        await EatAsync();

        // Release both forks
        await ReleaseForkAsync(_rightForkId);
        await ReleaseForkAsync(_leftForkId);
        _logger.LogInformation($"{_philosopherName} finished eating and released both forks");
    }

    private async Task<bool> TakeForkAsync(int forkId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_tableServiceUrl}/api/table/take-fork",
                new { PhilosopherId = _philosopherId, ForkId = forkId }
            );

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error taking fork {forkId}");
            return false;
        }
    }

    private async Task ReleaseForkAsync(int forkId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_tableServiceUrl}/api/table/release-fork",
                new { PhilosopherId = _philosopherId, ForkId = forkId }
            );

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error releasing fork {forkId}");
        }
    }

    private async Task EatAsync()
    {
        var eatingTime = _random.Next(40, 50);
        _totalEatingTime += eatingTime;
        _mealsEaten++;
        _logger.LogInformation($"{_philosopherName} is eating for {eatingTime}ms (Meal #{_mealsEaten})");
        await Task.Delay(eatingTime);

        // Update stats periodically
        if (_mealsEaten % 5 == 0)
        {
            await UpdateStatsAsync();
        }
    }

    private async Task UpdateStatsAsync()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_tableServiceUrl}/api/table/update-stats",
                new
                {
                    PhilosopherId = _philosopherId,
                    MealsEaten = _mealsEaten,
                    TotalThinkingTime = _totalThinkingTime,
                    TotalEatingTime = _totalEatingTime
                }
            );

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating stats");
        }
    }

    private async Task ExitAsync()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_tableServiceUrl}/api/table/exit",
                new { PhilosopherId = _philosopherId }
            );

            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"{_philosopherName} exited successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error exiting");
        }
    }
}


