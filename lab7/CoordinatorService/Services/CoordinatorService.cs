using System.Collections.Concurrent; // Можно убрать, если не используется
using Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Services;

public class ForkCoordinator
{
    private readonly ILogger<ForkCoordinator> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    
    // Используем обычные Dictionary
    private readonly Dictionary<int, string?> _forkOwners;
    private readonly Dictionary<string, PendingRequest> _pendingRequests;
    
    private readonly object _lockObject = new();
    private readonly int _philosophersCount;

    public ForkCoordinator(
        ILogger<ForkCoordinator> logger,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _philosophersCount = int.Parse(configuration["PHILOSOPHERS_COUNT"] ?? "5");
        
        // Исправлено: инициализируем как обычные Dictionary
        _forkOwners = new Dictionary<int, string?>();
        _pendingRequests = new Dictionary<string, PendingRequest>();

        // Initialize forks
        for (int i = 1; i <= _philosophersCount; i++)
        {
            _forkOwners[i] = null;
        }
    }

    public async Task HandleForkRequest(ForkRequestEvent request)
    {
        _logger.LogInformation(
            $"Received fork request from {request.PhilosopherName} ({request.PhilosopherId}) for forks {request.LeftForkId} and {request.RightForkId}");

        bool granted = false;
        string message;

        lock (_lockObject)
        {
            // 1. Проверяем доступность обеих вилок
            var leftOwner = _forkOwners.GetValueOrDefault(request.LeftForkId);
            var rightOwner = _forkOwners.GetValueOrDefault(request.RightForkId);

            bool leftAvailable = leftOwner == null;
            bool rightAvailable = rightOwner == null;

            if (leftAvailable && rightAvailable)
            {
                // 2. Грант: занимаем вилки
                _forkOwners[request.LeftForkId] = request.PhilosopherId;
                _forkOwners[request.RightForkId] = request.PhilosopherId;
                granted = true;
                message = $"Both forks {request.LeftForkId} and {request.RightForkId} granted";
                
                // Исправлено: просто Remove для Dictionary
                _pendingRequests.Remove(request.PhilosopherId);
            }
            else
            {
                // 3. Отказ: ставим в очередь
                _pendingRequests[request.PhilosopherId] = new PendingRequest
                {
                    Request = request,
                    RequestTime = DateTime.UtcNow
                };
                granted = false;
                message = $"Forks busy. Queued.";
            }
        }

        // 4. Отправляем ответ асинхронно
        if (granted)
        {
            _logger.LogInformation($"GRANTED: {request.PhilosopherName} took forks {request.LeftForkId} & {request.RightForkId}");
            
            // Исправлено: используем ForkPermissionEvent, так как ForkGrantedEvent не найден
            await _publishEndpoint.Publish(new ForkPermissionEvent 
            { 
                PhilosopherId = request.PhilosopherId,
                Granted = true, // Поле из ForkPermissionEvent
                Message = message
            });
        }
        else
        {
            _logger.LogInformation($"QUEUED: {request.PhilosopherName} waiting for {request.LeftForkId} & {request.RightForkId}");
        }
    }

    private int ExtractPhilosopherNumber(string philosopherId)
    {
        var parts = philosopherId.Split('-');
        if (parts.Length > 1 && int.TryParse(parts[1], out int number))
        {
            return number;
        }
        return int.MaxValue;
    }

    public async Task HandleForkRelease(ForkReleaseEvent releaseEvent)
    {
        _logger.LogInformation(
            $"Received fork release from {releaseEvent.PhilosopherId} for forks {releaseEvent.LeftForkId} and {releaseEvent.RightForkId}");

        List<PendingRequest> requestsToProcess = new();

        lock (_lockObject)
        {
            // Release forks
            if (_forkOwners.TryGetValue(releaseEvent.LeftForkId, out var leftOwner) &&
                leftOwner == releaseEvent.PhilosopherId)
            {
                _forkOwners[releaseEvent.LeftForkId] = null;
            }

            if (_forkOwners.TryGetValue(releaseEvent.RightForkId, out var rightOwner) &&
                rightOwner == releaseEvent.PhilosopherId)
            {
                _forkOwners[releaseEvent.RightForkId] = null;
            }

            // Исправлено: TryRemove -> Remove
            _pendingRequests.Remove(releaseEvent.PhilosopherId);

            // Collect requests
            var sortedRequests = _pendingRequests.Values
                .OrderBy(p => p.RequestTime)
                .ToList();

            foreach (var pending in sortedRequests)
            {
                var request = pending.Request;
                var leftAvailable = _forkOwners.TryGetValue(request.LeftForkId, out var leftOwner2) && leftOwner2 == null;
                var rightAvailable = _forkOwners.TryGetValue(request.RightForkId, out var rightOwner2) && rightOwner2 == null;

                if (leftAvailable && rightAvailable)
                {
                    // Grant
                    _forkOwners[request.LeftForkId] = request.PhilosopherId;
                    _forkOwners[request.RightForkId] = request.PhilosopherId;
                    requestsToProcess.Add(pending);
                    _logger.LogInformation(
                        $"Processing queued request from {request.PhilosopherName} - granting forks {request.LeftForkId} and {request.RightForkId}");
                }
            }

            // Remove processed requests
            foreach (var processed in requestsToProcess)
            {
                // Исправлено: TryRemove -> Remove
                _pendingRequests.Remove(processed.Request.PhilosopherId);
            }
        }

        // Publish outside lock
        foreach (var pending in requestsToProcess)
        {
            await _publishEndpoint.Publish(new ForkPermissionEvent
            {
                PhilosopherId = pending.Request.PhilosopherId,
                Granted = true,
                Message = $"Both forks {pending.Request.LeftForkId} and {pending.Request.RightForkId} granted"
            });
            _logger.LogInformation(
                $"Granted queued request from {pending.Request.PhilosopherName} for forks {pending.Request.LeftForkId} and {pending.Request.RightForkId}");
        }
    }

    // Класс PendingRequest лучше сделать public, если он используется в Generic'ах, но private тоже работает внутри класса
    public class PendingRequest
    {
        public ForkRequestEvent Request { get; set; } = null!;
        public DateTime RequestTime { get; set; }
    }
}
