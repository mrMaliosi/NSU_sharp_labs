using PhilosopherService.Services;
using PhilosopherService.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация из переменных окружения
var philosopherName = Environment.GetEnvironmentVariable("PHILOSOPHER_NAME") ?? "Unknown";
var philosopherId = Environment.GetEnvironmentVariable("PHILOSOPHER_ID") ?? "philosopher-1";
var leftForkId = int.Parse(Environment.GetEnvironmentVariable("LEFT_FORK_ID") ?? "1");
var rightForkId = int.Parse(Environment.GetEnvironmentVariable("RIGHT_FORK_ID") ?? "2");
var tableServiceUrl = Environment.GetEnvironmentVariable("TABLE_SERVICE_URL") ?? "http://table-service:8080";
var simulationDurationMinutes = int.Parse(Environment.GetEnvironmentVariable("SIMULATION_DURATION_MINUTES") ?? "5");

builder.Services.AddSingleton(new PhilosopherConfig
{
    Name = philosopherName,
    Id = philosopherId,
    LeftForkId = leftForkId,
    RightForkId = rightForkId,
    TableServiceUrl = tableServiceUrl,
    SimulationDurationMinutes = simulationDurationMinutes
});

builder.Services.AddHttpClient<TableServiceClient>();
builder.Services.AddSingleton<PhilosopherStateService>();
builder.Services.AddHostedService<PhilosopherHostedService>();
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

