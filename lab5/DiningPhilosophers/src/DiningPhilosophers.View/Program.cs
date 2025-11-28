using System;
using System.Globalization;
using System.Linq;
using DiningPhilosophers.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiningPhilosophers.View
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Использование: DiningPhilosophers.View --runId <GUID> --delay <секунды>");
                Console.WriteLine("Пример: DiningPhilosophers.View --runId 33 --delay 44.12");
                return;
            }

            Guid? runId = null;
            int? runIdInt = null;
            double? delay = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--runId" && i + 1 < args.Length)
                {
                    if (Guid.TryParse(args[i + 1], out var parsedRunId))
                    {
                        runId = parsedRunId;
                    }
                    else if (int.TryParse(args[i + 1], out var parsedInt))
                    {
                        runIdInt = parsedInt;
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: неверный формат RunId: {args[i + 1]}");
                        return;
                    }
                }
                else if (args[i] == "--delay" && i + 1 < args.Length)
                {
                    if (double.TryParse(args[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDelay))
                    {
                        delay = parsedDelay;
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: неверный формат delay: {args[i + 1]}");
                        return;
                    }
                }
            }

            if ((!runId.HasValue && !runIdInt.HasValue) || !delay.HasValue)
            {
                Console.WriteLine("Ошибка: необходимо указать --runId и --delay");
                return;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Ошибка: не найдена строка подключения DefaultConnection в appsettings.json");
                return;
            }

            var optionsBuilder = new DbContextOptionsBuilder<SimulationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new SimulationDbContext(optionsBuilder.Options);

            try
            {
                SimulationRun? simulationRun = null;
                
                if (runId.HasValue)
                {
                    simulationRun = context.SimulationRuns
                        .FirstOrDefault(r => r.RunId == runId.Value);
                    
                    if (simulationRun == null)
                    {
                        Console.WriteLine($"Ошибка: симуляция с RunId {runId.Value} не найдена");
                        return;
                    }
                }
                else if (runIdInt.HasValue)
                {
                    simulationRun = context.SimulationRuns
                        .FirstOrDefault(r => r.Id == runIdInt.Value);
                    
                    if (simulationRun == null)
                    {
                        Console.WriteLine($"Ошибка: симуляция с Id {runIdInt.Value} не найдена");
                        return;
                    }
                }

                var targetTime = simulationRun.StartTime.AddSeconds(delay.Value);

                // Получаем снимки состояния философов на указанный момент времени
                var philosopherSnapshots = context.PhilosopherSnapshots
                    .Where(p => p.SimulationRunId == simulationRun.Id && p.Timestamp <= targetTime)
                    .GroupBy(p => p.PhilosopherName)
                    .Select(g => g.OrderByDescending(p => p.Timestamp).First())
                    .ToList();

                // Получаем снимки состояния вилок на указанный момент времени
                var forkSnapshots = context.ForkSnapshots
                    .Where(f => f.SimulationRunId == simulationRun.Id && f.Timestamp <= targetTime)
                    .GroupBy(f => f.ForkId)
                    .Select(g => g.OrderByDescending(f => f.Timestamp).First())
                    .OrderBy(f => f.ForkId)
                    .ToList();

                Console.WriteLine($"===== СОСТОЯНИЕ СИМУЛЯЦИИ =====");
                Console.WriteLine($"RunId: {simulationRun.RunId}");
                Console.WriteLine($"Время начала симуляции: {simulationRun.StartTime:yyyy-MM-dd HH:mm:ss.fff}");
                Console.WriteLine($"Запрошенное время: {targetTime:yyyy-MM-dd HH:mm:ss.fff}");
                Console.WriteLine($"Смещение: {delay.Value} секунд");
                Console.WriteLine($"Стратегия: {simulationRun.Strategy}");
                Console.WriteLine();

                Console.WriteLine("Философы:");
                foreach (var snapshot in philosopherSnapshots.OrderBy(p => p.PhilosopherName))
                {
                    Console.WriteLine($"  {snapshot.PhilosopherName}: {snapshot.State} (Action = {snapshot.LastAction}), съедено: {snapshot.MealsEaten}");
                }
                Console.WriteLine();

                Console.WriteLine("Вилки:");
                foreach (var snapshot in forkSnapshots)
                {
                    if (!string.IsNullOrEmpty(snapshot.HeldByPhilosopherName))
                    {
                        Console.WriteLine($"  Fork-{snapshot.ForkId}: {snapshot.State} (используется {snapshot.HeldByPhilosopherName})");
                    }
                    else
                    {
                        Console.WriteLine($"  Fork-{snapshot.ForkId}: {snapshot.State}");
                    }
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении состояния: {ex.Message}");
                Console.WriteLine($"Детали: {ex}");
            }
        }
    }
}

