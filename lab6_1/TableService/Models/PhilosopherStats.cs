namespace TableService.Models;

public class PhilosopherStats
{
    public string PhilosopherId { get; set; } = string.Empty;
    public string PhilosopherName { get; set; } = string.Empty;
    public int MealsEaten { get; set; }
    public int TotalThinkingTime { get; set; }
    public int TotalEatingTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}


