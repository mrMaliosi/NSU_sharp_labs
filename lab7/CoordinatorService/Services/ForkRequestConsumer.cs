using Shared.Messages;
using CoordinatorService.Services;
using MassTransit;

namespace CoordinatorService.Services;

public class ForkRequestConsumer : IConsumer<ForkRequestEvent>
{
    private readonly ForkCoordinator _forkCoordinator;
    private readonly ILogger<ForkRequestConsumer> _logger;

    public ForkRequestConsumer(
        ForkCoordinator forkCoordinator,
        ILogger<ForkRequestConsumer> logger)
    {
        _forkCoordinator = forkCoordinator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ForkRequestEvent> context)
    {
        _logger.LogInformation($"ForkRequestConsumer received event from {context.Message.PhilosopherId}");
        await _forkCoordinator.HandleForkRequest(context.Message);
    }
}

