using System.Net.Http.Json;
using MassTransit;
using Shared.Messages;

namespace PhilosopherService.Services;

public class PhilosopherWorker
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PhilosopherWorker> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
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
    private int _totalHungryTime = 0;
    
    // Event-driven coordination
    private readonly SemaphoreSlim _permissionSemaphore = new(0, 1);
    private bool _permissionGranted = false;
    private string _permissionMessage = string.Empty;

    public PhilosopherWorker(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PhilosopherWorker> logger,
        IPublishEndpoint publishEndpoint)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _logger = logger;
        _publishEndpoint = publishEndpoint;

        _philosopherId = configuration["PHILOSOPHER_ID"] ?? throw new InvalidOperationException("PHILOSOPHER_ID not set");
        _philosopherName = configuration["PHILOSOPHER_NAME"] ?? throw new InvalidOperationException("PHILOSOPHER_NAME not set");
        _leftForkId = int.Parse(configuration["LEFT_FORK_ID"] ?? throw new InvalidOperationException("LEFT_FORK_ID not set"));
        _rightForkId = int.Parse(configuration["RIGHT_FORK_ID"] ?? throw new InvalidOperationException("RIGHT_FORK_ID not set"));
        _tableServiceUrl = configuration["TABLE_SERVICE_URL"] ?? throw new InvalidOperationException("TABLE_SERVICE_URL not set");
        _simulationDurationMinutes = int.Parse(configuration["SIMULATION_DURATION_MINUTES"] ?? "5");
    }

    public void HandlePermission(ForkPermissionEvent permissionEvent)
    {
        if (permissionEvent.PhilosopherId == _philosopherId)
        {
            _permissionGranted = permissionEvent.Granted;
            _permissionMessage = permissionEvent.Message;
            _logger.LogInformation($"{_philosopherName} received permission: {permissionEvent.Message}");
            _permissionSemaphore.Release();
        }
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
        var thinkingTime = _random.Next(300, 1000);
        _totalThinkingTime += thinkingTime;
        _logger.LogDebug($"{_philosopherName} is thinking for {thinkingTime}ms");
        await Task.Delay(thinkingTime);
    }

    private async Task TryEatAsync()
    {
        _logger.LogInformation($"{_philosopherName} requesting permission for forks {_leftForkId} and {_rightForkId} from coordinator");

        var hungryStartTime = DateTime.UtcNow;

        // 1. Очистка состояния перед запросом
        _permissionGranted = false;
        _permissionMessage = string.Empty;
        
        // Сброс семафора (drain)
        while (_permissionSemaphore.CurrentCount > 0)
        {
            _permissionSemaphore.Wait(0);
        }

        // 2. Отправка запроса Координатору
        var requestEvent = new ForkRequestEvent
        {
            PhilosopherId = _philosopherId,
            PhilosopherName = _philosopherName,
            LeftForkId = _leftForkId,
            RightForkId = _rightForkId,
            RequestTime = hungryStartTime
        };
        
        try
        {
            await _publishEndpoint.Publish(requestEvent);
            _logger.LogInformation($"{_philosopherName} waiting for coordinator response...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_philosopherName} failed to publish fork request");
            // Ждем немного перед ретраем, чтобы не спамить логами в цикле
            await Task.Delay(1000); 
            return; 
        }

        // 3. Ожидание ответа (таймаут лучше сделать разумным, например, 30 сек)
        var timeout = TimeSpan.FromSeconds(30); 
        if (!await _permissionSemaphore.WaitAsync(timeout))
        {
            // Важно: Если мы не дождались ответа, мы не знаем состояние Координатора.
            // Лучше на всякий случай отправить Release, вдруг Координатор дал добро, 
            // но сообщение потерялось/задержалось, и у него вилки теперь "заняты" нами.
            _logger.LogWarning($"{_philosopherName} TIMEOUT waiting for coordinator. Sending fail-safe release.");
            
            await _publishEndpoint.Publish(new ForkReleaseEvent
            {
                PhilosopherId = _philosopherId,
                LeftForkId = _leftForkId,
                RightForkId = _rightForkId,
                ReleaseTime = DateTime.UtcNow
            });
            return;
        }

        if (!_permissionGranted)
        {
            _logger.LogDebug($"{_philosopherName} denied/queued by coordinator: {_permissionMessage}");
            return; // Координатор сам держит нас в очереди или отказал
        }

        _logger.LogInformation($"{_philosopherName} GRANTED by coordinator. Taking physical forks...");

        // 4. Физический захват (HTTP к TableService)
        // Раз Координатор разрешил, считаем, что вилки наши. TableService просто уведомляем.
        
        var leftForkTaken = await TakeForkAsync(_leftForkId);
        if (!leftForkTaken)
        {
            // Критическая рассинхронизация: Координатор дал добро, а Стол - нет.
            _logger.LogError($"{_philosopherName} failed to take Left Fork {_leftForkId} from TableService (Consistency Error!)");
            
            // Откат транзакции
            await _publishEndpoint.Publish(new ForkReleaseEvent
            {
                PhilosopherId = _philosopherId,
                LeftForkId = _leftForkId,
                RightForkId = _rightForkId,
                ReleaseTime = DateTime.UtcNow
            });
            return;
        }

        var rightForkTaken = await TakeForkAsync(_rightForkId);
        if (!rightForkTaken)
        {
            _logger.LogError($"{_philosopherName} failed to take Right Fork {_rightForkId} from TableService (Consistency Error!)");
            
            // Откат: возвращаем левую
            await ReleaseForkAsync(_leftForkId);
            
            // Уведомляем координатора
            await _publishEndpoint.Publish(new ForkReleaseEvent
            {
                PhilosopherId = _philosopherId,
                LeftForkId = _leftForkId,
                RightForkId = _rightForkId,
                ReleaseTime = DateTime.UtcNow
            });
            return;
        }

        var waitDuration = DateTime.UtcNow - hungryStartTime;
        _totalHungryTime += (int)waitDuration.TotalMilliseconds;

        // 5. Едим
        _logger.LogInformation($"{_philosopherName} EATING...");
        await EatAsync();

        // 6. Завершение
        // Сначала освобождаем физически (чтобы другие видели в UI)
        await ReleaseForkAsync(_rightForkId);
        await ReleaseForkAsync(_leftForkId);
        
        // Потом сообщаем мозгу (Координатору)
        await _publishEndpoint.Publish(new ForkReleaseEvent
        {
            PhilosopherId = _philosopherId,
            LeftForkId = _leftForkId,
            RightForkId = _rightForkId,
            ReleaseTime = DateTime.UtcNow
        });
        
        _logger.LogInformation($"{_philosopherName} DONE eating.");
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
        var eatingTime = _random.Next(400, 600);
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
                    TotalEatingTime = _totalEatingTime,
                    TotalHungryTime = _totalHungryTime
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

    public bool IsForMe(string targetPhilosopherId)
    {
        return _philosopherId == targetPhilosopherId;
    }
}


