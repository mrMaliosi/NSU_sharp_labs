using PhilosopherService.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Получаем ID философа для имени очереди
var philosopherId = builder.Configuration["PHILOSOPHER_ID"] ?? Guid.NewGuid().ToString();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ForkPermissionConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RABBITMQ_HOST"] ?? "rabbitmq";
        var rabbitMqUser = builder.Configuration["RABBITMQ_USER"] ?? "guest";
        var rabbitMqPassword = builder.Configuration["RABBITMQ_PASSWORD"] ?? "guest";

        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUser);
            h.Password(rabbitMqPassword);
        });

        // ВМЕСТО cfg.ConfigureEndpoints(context);
        // Создаем уникальную очередь для каждого философа
        // Например: "fork-permission-philosopher-1"
        cfg.ReceiveEndpoint($"fork-permission-{philosopherId}", e =>
        {
            e.ConfigureConsumer<ForkPermissionConsumer>(context);
        });
    });
});


builder.Services.AddSingleton<PhilosopherWorker>();
builder.Services.AddHostedService<PhilosopherBackgroundService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

