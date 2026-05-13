using CoordinatorService.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ForkRequestConsumer>();
    x.AddConsumer<ForkReleaseConsumer>();

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

        cfg.ConfigureEndpoints(context);
    });
});

// Register ForkCoordinator as a singleton
builder.Services.AddSingleton<ForkCoordinator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

