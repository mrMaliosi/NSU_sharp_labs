using Lab1.DiningPhilosophers;
using Microsoft.Extensions.Hosting;

namespace PhilosopherService.Services;

public class PhilosopherHostedService : BackgroundService
{
    private readonly PhilosopherConfig _config;
    private readonly TableServiceClient _tableServiceClient;
    private readonly PhilosopherStateService _stateService;
    private readonly ILogger<PhilosopherHostedService> _logger;
    private readonly PhilosopherContext _context;
    private readonly Random _random = new Random();
    private DateTime _simulationStartTime;
    private DateTime _lastWaitingTimeUpdate = DateTime.Now;

    public PhilosopherHostedService(
        PhilosopherConfig config,
        TableServiceClient tableServiceClient,
        PhilosopherStateService stateService,
        ILogger<PhilosopherHostedService> logger)
    {
        _config = config;
        _tableServiceClient = tableServiceClient;
        _stateService = stateService;
        _logger = logger;
        
        // Создаем контекст философа с дефолтными значениями
        _context = new PhilosopherContext(
            new Segment(30, 100),
            new Segment(40, 50),
            20);
        
        _stateService.ChangeState(PhilosopherState.Thinking, _context.GetThinkingDuration());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _simulationStartTime = DateTime.Now;
        _logger.LogInformation("Философ {Name} начал работу", _config.Name);

        // Регистрируемся в TableService
        await _tableServiceClient.RegisterPhilosopherAsync(_config.Id, _config.Name);

        var simulationDurationMs = _config.SimulationDurationMinutes * 60 * 1000;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Проверяем, не истекло ли время симуляции
                var elapsed = (DateTime.Now - _simulationStartTime).TotalMilliseconds;
                if (elapsed >= simulationDurationMs)
                {
                    _logger.LogInformation("Время симуляции истекло для философа {Name}", _config.Name);
                    break;
                }

                _stateService.RealiseThePassageOfTime();
                
                // Проверяем переходы состояний после обновления времени
                if (!_stateService.IsBusy())
                {
                    await PerformActionAsync();
                }
                
                // Обновляем время ожидания в TableService периодически (каждую секунду)
                if (_stateService.State == PhilosopherState.Hungry && 
                    (DateTime.Now - _lastWaitingTimeUpdate).TotalSeconds >= 1)
                {
                    await _tableServiceClient.UpdateWaitingTimeAsync(_config.Name, _stateService.TotalWaitingTimeMs);
                    _lastWaitingTimeUpdate = DateTime.Now;
                }
                await Task.Delay(1, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в работе философа {Name}", _config.Name);
            }
        }

        // Освобождаем вилки перед выходом
        if (_stateService.LeftForkAcquired)
        {
            await _tableServiceClient.ReleaseForkAsync(_config.LeftForkId);
        }
        if (_stateService.RightForkAcquired)
        {
            await _tableServiceClient.ReleaseForkAsync(_config.RightForkId);
        }

        // Уведомляем TableService о выходе
        await _tableServiceClient.PhilosopherExitAsync(_config.Id);
        _logger.LogInformation("Философ {Name} завершил работу", _config.Name);
    }

    private async Task PerformActionAsync()
    {
        switch (_stateService.State)
        {
            case PhilosopherState.Thinking:
                // Переход в состояние Hungry
                _stateService.ChangeState(PhilosopherState.Hungry, 0);
                break;

            case PhilosopherState.Hungry:
                await TryToPickForkAsync();
                CheckFutilityOfBeing();
                break;

            case PhilosopherState.Eating:
                // Переход в состояние Thinking после завершения еды
                await ReleaseForksAsync();
                await _tableServiceClient.RecordMealAsync(_config.Name);
                _stateService.ChangeState(PhilosopherState.Thinking, _context.GetThinkingDuration());
                break;
        }
    }

    private async Task TryToPickForkAsync()
    {
        int chooseFork = _random.Next(2);
        bool success = false;

        if (chooseFork == 0 && !_stateService.LeftForkAcquired)
        {
            success = await _tableServiceClient.AcquireForkAsync(_config.LeftForkId, _config.Id, _config.Name);
            if (success)
            {
                _stateService.SetLeftForkAcquired(true);
            }
        }
        else if (chooseFork == 1 && !_stateService.RightForkAcquired)
        {
            success = await _tableServiceClient.AcquireForkAsync(_config.RightForkId, _config.Id, _config.Name);
            if (success)
            {
                _stateService.SetRightForkAcquired(true);
            }
        }

        // Если обе вилки захвачены, переходим в состояние Eating
        if (_stateService.LeftForkAcquired && _stateService.RightForkAcquired)
        {
            _stateService.ChangeState(PhilosopherState.Eating, _context.GetEatingDuration());
        }
    }

    private void CheckFutilityOfBeing()
    {
        // Реализация логики из NaiveStrategy
        if (-_context.eatingTime.To * 10 > _stateService.MillisecondsRemainingInState)
        {
            int chance = _random.Next(100);
            if (chance < -_stateService.MillisecondsRemainingInState - _context.eatingTime.To * 10)
            {
                ReleaseForksAsync().Wait();
            }
        }
    }

    private async Task ReleaseForksAsync()
    {
        if (_stateService.LeftForkAcquired)
        {
            await _tableServiceClient.ReleaseForkAsync(_config.LeftForkId);
        }
        if (_stateService.RightForkAcquired)
        {
            await _tableServiceClient.ReleaseForkAsync(_config.RightForkId);
        }
        _stateService.ReleaseForks();
    }
}

