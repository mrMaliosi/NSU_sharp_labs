using MassTransit;
using Shared.Messages;
using PhilosopherService.Services;

namespace PhilosopherService.Services;

public class ForkPermissionConsumer : IConsumer<ForkPermissionEvent>
{
    private readonly PhilosopherWorker _philosopherWorker;
    private readonly ILogger<ForkPermissionConsumer> _logger;

    public ForkPermissionConsumer(
        PhilosopherWorker philosopherWorker,
        ILogger<ForkPermissionConsumer> logger)
    {
        _philosopherWorker = philosopherWorker;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ForkPermissionEvent> context)
    {
        if (!_philosopherWorker.IsForMe(context.Message.PhilosopherId))
        {
            // Это сообщение не для нас.
            return Task.CompletedTask;
        }

        _logger.LogInformation($"ForkPermissionConsumer received event for {context.Message.PhilosopherId}, Granted={context.Message.Granted}");
        _philosopherWorker.HandlePermission(context.Message);
        return Task.CompletedTask;
    }

}

