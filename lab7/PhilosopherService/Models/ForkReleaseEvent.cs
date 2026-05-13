namespace Shared.Messages;

public class ForkReleaseEvent
{
    public string PhilosopherId { get; set; } = string.Empty;
    public int LeftForkId { get; set; }
    public int RightForkId { get; set; }
    public DateTime ReleaseTime { get; set; } = DateTime.UtcNow;
}

