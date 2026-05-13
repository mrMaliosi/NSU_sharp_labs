using Shared.Messages;
using CoordinatorService.Services;
using MassTransit;

namespace CoordinatorService.Services;

public class ForkReleaseConsumer : IConsumer<ForkReleaseEvent>
{
    private readonly ForkCoordinator _forkCoordinator;
    private readonly ILogger<ForkReleaseConsumer> _logger;

    public ForkReleaseConsumer(
        ForkCoordinator forkCoordinator,
        ILogger<ForkReleaseConsumer> logger)
    {
        _forkCoordinator = forkCoordinator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ForkReleaseEvent> context)
    {
        await _forkCoordinator.HandleForkRelease(context.Message);
    }
}

