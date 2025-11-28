# Руководство разработчика: Симуляция обедающих философов

## Содержание

1. [Обзор архитектуры](#обзор-архитектуры)
2. [Структура проекта](#структура-проекта)
3. [Основные компоненты](#основные-компоненты)
4. [Жизненный цикл приложения](#жизненный-цикл-приложения)
5. [Конфигурация](#конфигурация)
6. [Расширение функциональности](#расширение-функциональности)
7. [Dependency Injection](#dependency-injection)
8. [Логирование](#логирование)

---

## Обзор архитектуры

Проект реализует классическую задачу "Обедающие философы" с использованием современного подхода .NET Generic Host. Каждый философ представлен отдельным `BackgroundService`, что обеспечивает параллельное выполнение и правильное управление жизненным циклом.

### Ключевые особенности:

- **.NET Generic Host** - управление жизненным циклом приложения
- **BackgroundService** - каждый философ работает в отдельном фоновом сервисе
- **Dependency Injection** - все компоненты регистрируются через DI контейнер
- **IOptions Pattern** - конфигурация через `appsettings.json`
- **IHostApplicationLifetime** - управление временем работы приложения
- **CancellationToken** - корректная остановка всех сервисов

---

## Структура проекта

```
DiningPhilosophers/
├── src/
│   ├── DiningPhilosophers.App/          # Главное приложение
│   │   ├── Program.cs                    # Точка входа, настройка Host
│   │   ├── appsettings.json              # Конфигурация
│   │   └── Services/                     # Hosted Services
│   │       ├── PhilosopherHostedService.cs
│   │       ├── DisplayHostedService.cs
│   │       ├── SimulationLifecycleService.cs
│   │       └── PhilosopherHostedServiceFactory.cs
│   │
│   ├── DiningPhilosophers.Core/          # Основная бизнес-логика
│   │   ├── Objects/                      # Доменные объекты
│   │   │   ├── Philosopher.cs
│   │   │   ├── Fork.cs
│   │   │   └── Metrics.cs
│   │   ├── Services/                     # Сервисы
│   │   │   ├── IForkManager.cs / ForkManager.cs
│   │   │   ├── IMetricsCalculator.cs / MetricsCalculator.cs
│   │   │   └── IDisplayService.cs / DisplayService.cs
│   │   ├── Configuration/                # Классы конфигурации
│   │   │   ├── SimulationOptions.cs
│   │   │   └── PhilosophersOptions.cs
│   │   └── Contexts/                     # Контексты
│   │       └── PhilosopherContext.cs
│   │
│   └── DiningPhilosophers.Strategies/   # Стратегии поведения
│       ├── Contracts/
│       │   └── IPhilosopherStrategy.cs
│       └── NaiveStrategy.cs
│
└── DiningPhilosophers.sln
```

---

## Основные компоненты

### 1. PhilosopherHostedService

Каждый философ работает в отдельном `BackgroundService`. Это обеспечивает:

- Параллельное выполнение всех философов
- Независимое управление жизненным циклом
- Корректную обработку `CancellationToken`

```csharp
public sealed class PhilosopherHostedService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _strategy.PerformAction(_philosopher, _allPhilosophers, _allForks);
            _metricsCalculator.OnStep(_allForks, _allPhilosophers);
            await Task.Delay(1, stoppingToken);
        }
    }
}
```

### 2. SimulationLifecycleService

Управляет временем работы симуляции через `IHostApplicationLifetime`:

```csharp
public sealed class SimulationLifecycleService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(OnTimerElapsed, null, _options.SimulationDurationMs, Timeout.Infinite);
        return Task.CompletedTask;
    }
    
    private void OnTimerElapsed(object? state)
    {
        _displayService.DisplayMetrics(_philosophers, _metricsCalculator);
        _lifetime.StopApplication(); // Корректно останавливает все сервисы
    }
}
```

### 3. DisplayHostedService

Периодически выводит статистику состояния философов и вилок.

### 4. ForkManager

Управляет всеми вилками в системе. Реализует паттерн Singleton через DI.

### 5. MetricsCalculator

Собирает и вычисляет метрики:
- Пропускная способность (блюд/мс)
- Среднее время ожидания по философам
- Коэффициент утилизации вилок

### 6. IPhilosopherStrategy

Интерфейс для стратегий поведения философов. Позволяет легко добавлять новые алгоритмы.

---

## Жизненный цикл приложения

1. **Инициализация** (`Program.Main`)
   - Создание `IHostBuilder` через `Host.CreateDefaultBuilder()`
   - Загрузка конфигурации из `appsettings.json`
   - Регистрация всех сервисов в DI контейнере

2. **Запуск** (`host.Run()`)
   - Запуск всех зарегистрированных `IHostedService`
   - Каждый философ начинает работать в своем потоке
   - Запускается сервис отображения
   - Запускается сервис управления жизненным циклом

3. **Работа**
   - Философы выполняют действия согласно стратегии
   - Метрики обновляются на каждом шаге
   - Статистика выводится периодически

4. **Остановка**
   - `SimulationLifecycleService` вызывает `StopApplication()` после истечения времени
   - Все сервисы получают `CancellationToken` с запросом на отмену
   - Выводятся финальные метрики
   - Приложение корректно завершается

---

## Конфигурация

### appsettings.json

```json
{
  "Simulation": {
    "SimulationDurationMs": 10000,    // Длительность симуляции в мс
    "DisplayIntervalMs": 150,         // Интервал вывода статистики
    "TotalPhilosophers": 5,           // Количество философов
    "TotalForks": 5,                  // Количество вилок
    "Strategy": "naive"               // Название стратегии
  },
  "Philosophers": {
    "Names": ["Платон", "Аристотель", ...],
    "DefaultContext": {
      "ThinkingTime": { "From": 30, "To": 100 },
      "EatingTime": { "From": 40, "To": 50 },
      "ForkPickTime": 20
    }
  }
}
```

### Использование IOptions

Конфигурация загружается через паттерн `IOptions<T>`:

```csharp
services.Configure<SimulationOptions>(
    configuration.GetSection(SimulationOptions.SectionName));
```

В сервисах используется через внедрение зависимости:

```csharp
public DisplayHostedService(IOptions<SimulationOptions> options)
{
    _options = options.Value;
}
```

---

## Расширение функциональности

### Добавление новой стратегии

1. Создайте класс, реализующий `IPhilosopherStrategy`:

```csharp
public sealed class MyNewStrategy : IPhilosopherStrategy
{
    public void PerformAction(
        Philosopher philosopher, 
        Philosopher[] allPhilosophers, 
        Fork[] allForks)
    {
        // Ваша логика здесь
        if (philosopher.State == PhilosopherState.Hungry)
        {
            // Попытка взять вилки
            philosopher.PickLeftFork();
            philosopher.PickRightFork();
        }
        
        philosopher.RealiseThePassageOfTime();
    }
}
```

2. Зарегистрируйте стратегию в `Program.cs`:

```csharp
IPhilosopherStrategy strategy = simulationOptions.Strategy.ToLowerInvariant() switch
{
    "naive" => new NaiveStrategy(),
    "mynew" => new MyNewStrategy(),  // Добавьте здесь
    _ => new NaiveStrategy()
};
services.AddSingleton<IPhilosopherStrategy>(strategy);
```

3. Укажите стратегию в `appsettings.json`:

```json
{
  "Simulation": {
    "Strategy": "mynew"
  }
}
```

### Добавление нового сервиса

1. Создайте интерфейс и реализацию:

```csharp
public interface IMyService
{
    void DoSomething();
}

public sealed class MyService : IMyService
{
    public void DoSomething() { }
}
```

2. Зарегистрируйте в `Program.cs`:

```csharp
services.AddSingleton<IMyService, MyService>();
```

3. Используйте через DI в других сервисах:

```csharp
public PhilosopherHostedService(IMyService myService)
{
    _myService = myService;
}
```

### Добавление нового Hosted Service

1. Создайте класс, наследующий `BackgroundService`:

```csharp
public sealed class MyHostedService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Ваша логика
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

2. Зарегистрируйте в `Program.cs`:

```csharp
services.AddHostedService<MyHostedService>();
```

---

## Dependency Injection

### Регистрация сервисов

Все сервисы регистрируются в методе `ConfigureServices`:

```csharp
.ConfigureServices((context, services) =>
{
    // Singleton - один экземпляр на все приложение
    services.AddSingleton<IForkManager, ForkManager>();
    
    // Scoped - один экземпляр на область (не используется в Hosted Services)
    // services.AddScoped<IService, Service>();
    
    // Transient - новый экземпляр при каждом запросе
    // services.AddTransient<IService, Service>();
    
    // Hosted Services - автоматически управляются Host'ом
    services.AddHostedService<PhilosopherHostedService>();
})
```

### Внедрение зависимостей

Зависимости внедряются через конструктор:

```csharp
public PhilosopherHostedService(
    Philosopher philosopher,
    IPhilosopherStrategy strategy,
    IMetricsCalculator metricsCalculator,
    ILogger<PhilosopherHostedService> logger)
{
    // DI контейнер автоматически предоставит все зависимости
}
```

### Регистрация нескольких экземпляров одного типа

Для регистрации нескольких `PhilosopherHostedService` используется фабрика:

```csharp
foreach (var philosopher in philosophers)
{
    services.AddSingleton<IHostedService>(sp => 
        PhilosopherHostedServiceFactory.Create(sp, philosopher));
}
```

---

## Логирование

### Настройка логирования

В `Program.cs`:

```csharp
.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
})
```

### Использование в сервисах

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
    
    public void DoWork()
    {
        _logger.LogInformation("Начало работы");
        _logger.LogError("Ошибка: {Error}", errorMessage);
        _logger.LogWarning("Предупреждение");
    }
}
```

### Уровни логирования

- `LogLevel.Trace` - детальная отладочная информация
- `LogLevel.Debug` - отладочная информация
- `LogLevel.Information` - общая информация
- `LogLevel.Warning` - предупреждения
- `LogLevel.Error` - ошибки
- `LogLevel.Critical` - критические ошибки

---

## Работа с CancellationToken

Все асинхронные операции должны проверять `CancellationToken`:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        // Работа
        await Task.Delay(1000, stoppingToken); // Автоматически отменится при запросе
    }
}
```

Обработка отмены:

```csharp
try
{
    await SomeWorkAsync(stoppingToken);
}
catch (OperationCanceledException)
{
    // Корректная обработка отмены
    _logger.LogInformation("Работа была отменена");
}
```

---

## Метрики и статистика

### Доступные метрики

1. **Пропускная способность** - количество блюд на миллисекунду
2. **Среднее время ожидания** - среднее время ожидания вилок для каждого философа
3. **Утилизация вилок** - процент времени использования каждой вилки

### Получение метрик

```csharp
var metrics = serviceProvider.GetRequiredService<IMetricsCalculator>();

double throughput = metrics.GetThroughput();
Dictionary<string, double> avgWaiting = metrics.GetAverageWaitingTime();
Dictionary<int, double> forkUtil = metrics.GetForkUtilization();
```

---

## Отладка

### Проверка конфигурации

Добавьте временный код в `Program.cs` для проверки загрузки конфигурации:

```csharp
var config = context.Configuration;
var allKeys = config.AsEnumerable().Select(kvp => kvp.Key);
foreach (var key in allKeys)
{
    Console.WriteLine($"{key} = {config[key]}");
}
```

### Логирование состояния

Используйте логирование для отслеживания состояния:

```csharp
_logger.LogDebug("Философ {Name} в состоянии {State}", 
    philosopher.Name, philosopher.State);
```

---

## Лучшие практики

1. **Всегда используйте CancellationToken** в асинхронных операциях
2. **Регистрируйте сервисы как Singleton** для Hosted Services (они живут все время работы приложения)
3. **Используйте IOptions<T>** для конфигурации вместо прямого чтения
4. **Логируйте важные события** для отладки
5. **Обрабатывайте исключения** в циклах Hosted Services
6. **Не блокируйте ExecuteAsync** - используйте async/await

---

## Примеры использования

### Изменение количества философов

В `appsettings.json`:

```json
{
  "Simulation": {
    "TotalPhilosophers": 10,
    "TotalForks": 10
  }
}
```

### Изменение длительности симуляции

```json
{
  "Simulation": {
    "SimulationDurationMs": 30000  // 30 секунд
  }
}
```

### Настройка интервала отображения

```json
{
  "Simulation": {
    "DisplayIntervalMs": 500  // Каждые 500 мс
  }
}
```

---

## Решение проблем

### Проблема: Конфигурация не загружается

**Решение:**
1. Убедитесь, что `appsettings.json` находится в корне проекта `DiningPhilosophers.App`
2. Проверьте, что файл копируется в выходную директорию (настройка в `.csproj`)
3. Проверьте синтаксис JSON

### Проблема: Сервисы не запускаются

**Решение:**
1. Проверьте регистрацию в `ConfigureServices`
2. Убедитесь, что все зависимости зарегистрированы
3. Проверьте логи на наличие ошибок

### Проблема: Философы не работают параллельно

**Решение:**
1. Убедитесь, что каждый философ зарегистрирован как отдельный `IHostedService`
2. Проверьте, что используется `async/await`, а не блокирующие операции

---

## Дополнительные ресурсы

- [.NET Generic Host](https://docs.microsoft.com/dotnet/core/extensions/generic-host)
- [Background Services](https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [Dependency Injection в .NET](https://docs.microsoft.com/dotnet/core/extensions/dependency-injection)
- [Configuration в .NET](https://docs.microsoft.com/dotnet/core/extensions/configuration)

---

**Версия документа:** 1.0  
**Дата обновления:** 2024

