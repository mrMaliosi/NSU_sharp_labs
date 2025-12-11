namespace PhilosopherService.Models;

public class AcquireForkRequest
{
    public string PhilosopherId { get; set; } = string.Empty;
    public string PhilosopherName { get; set; } = string.Empty;
}

public class AcquireForkResponse
{
    public bool Success { get; set; }
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

