namespace TableService.Models;

public class AcquireForkRequest
{
    public string PhilosopherId { get; set; } = string.Empty;
    public string PhilosopherName { get; set; } = string.Empty;
}

public class AcquireForkResponse
{
    public bool Success { get; set; }
}

public class ForkStateResponse
{
    public int ForkId { get; set; }
    public string State { get; set; } = string.Empty;
    public string? HeldBy { get; set; }
}

public class RecordMealRequest
{
    public string PhilosopherName { get; set; } = string.Empty;
}

public class UpdateWaitingTimeRequest
{
    public string PhilosopherName { get; set; } = string.Empty;
    public long WaitingTimeMs { get; set; }
}

public class RegisterPhilosopherRequest
{
    public string PhilosopherId { get; set; } = string.Empty;
    public string PhilosopherName { get; set; } = string.Empty;
}

public class MetricsResponse
{
    public long TotalSimulationTimeMs { get; set; }
    public double Throughput { get; set; }
    public Dictionary<string, double> AverageWaitingTimes { get; set; } = new();
    public Dictionary<int, double> ForkUtilization { get; set; } = new();
    public Dictionary<string, int> MealsByPhilosopher { get; set; } = new();
}

