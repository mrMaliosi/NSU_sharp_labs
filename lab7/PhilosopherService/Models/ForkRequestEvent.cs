namespace Shared.Messages;

public class ForkRequestEvent
{
    public string PhilosopherId { get; set; } = string.Empty;
    public string PhilosopherName { get; set; } = string.Empty;
    public int LeftForkId { get; set; }
    public int RightForkId { get; set; }
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;
}

