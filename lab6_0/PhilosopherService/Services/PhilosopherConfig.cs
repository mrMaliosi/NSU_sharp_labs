namespace PhilosopherService.Services;

public class PhilosopherConfig
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public int LeftForkId { get; set; }
    public int RightForkId { get; set; }
    public string TableServiceUrl { get; set; } = string.Empty;
    public int SimulationDurationMinutes { get; set; }
}

