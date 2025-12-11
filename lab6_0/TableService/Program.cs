using Lab1.DiningPhilosophers;
using TableService.Services;
using TableService.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация
var philosophersCount = int.Parse(Environment.GetEnvironmentVariable("PHILOSOPHERS_COUNT") ?? "5");
var totalForks = philosophersCount;

// Регистрация сервисов
var forkManager = new ForkManager(totalForks);
builder.Services.AddSingleton<IForkManager>(forkManager);

var metricsCalculator = new MetricsCalculator();
metricsCalculator.StartSimulation(forkManager.GetAllForks());
builder.Services.AddSingleton<IMetricsCalculator>(metricsCalculator);

builder.Services.AddSingleton<TableStateService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Логирование
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

