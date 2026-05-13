using Lab1.DiningPhilosophers;
using TableService.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрируем сервисы приложения
var philosophersCount = int.Parse(Environment.GetEnvironmentVariable("PHILOSOPHERS_COUNT") ?? "5");
var forkManager = new ForkManager(philosophersCount);
builder.Services.AddSingleton<IForkManager>(forkManager);

var metricsCalculator = new MetricsCalculator();
metricsCalculator.StartSimulation(forkManager.GetAllForks());
builder.Services.AddSingleton<IMetricsCalculator>(metricsCalculator);

builder.Services.AddSingleton<TableStateService>();

var app = builder.Build();

// Настраиваем конвейер HTTP запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

app.Run();

