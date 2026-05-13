namespace Shared.Messages;

public class ForkPermissionEvent
{
    public string PhilosopherId { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime PermissionTime { get; set; } = DateTime.UtcNow;
}

