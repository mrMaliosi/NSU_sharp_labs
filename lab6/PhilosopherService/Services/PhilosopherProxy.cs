namespace TableService.Services;

public class PhilosopherProxy
{
    public string Id { get; }
    public string Name { get; }
    public int MealsEaten { get; set; }
    public long TotalWaitingTimeMs { get; set; }

    public PhilosopherProxy(string id, string name)
    {
        Id = id;
        Name = name;
        MealsEaten = 0;
        TotalWaitingTimeMs = 0;
    }
}

