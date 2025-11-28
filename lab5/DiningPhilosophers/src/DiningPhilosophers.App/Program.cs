using System;
using System.Linq;
using Lab1.DiningPhilosophers;
using DiningPhilosophers.App.Services;
using DiningPhilosophers.Core.Data;
using DiningPhilosophers.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiningPhilosophers.App
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Program");
            logger.LogInformation("Запуск симуляции обедающих философов...");

            // Подписка на события философов после создания host
            var philosophers = host.Services.GetRequiredService<Philosopher[]>();
            var metrics = host.Services.GetRequiredService<IMetricsCalculator>();
            foreach (var philosopher in philosophers)
            {
                philosopher.OnMealEaten += (name) => metrics.OnMeal(name);
            }

            // Инициализация сохранения состояния
            using (var scope = host.Services.CreateScope())
            {
                var stateService = scope.ServiceProvider.GetRequiredService<ISimulationStateService>();
                var simulationOptions = host.Services.GetRequiredService<IOptions<SimulationOptions>>().Value;
                var forkManager = host.Services.GetRequiredService<IForkManager>();
                
                var runId = stateService.StartSimulation(
                    simulationOptions.TotalPhilosophers,
                    simulationOptions.TotalForks,
                    simulationOptions.Strategy);
                
                logger.LogInformation("Симуляция запущена с RunId: {RunId}", runId);
            }

            try
            {
                host.Run();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Приложение успешно антиоживилось из-за успешной ошибки, непременно говорящей, что всё прошло успешно: {ErrorMessage}", ex.Message);
            }
            finally
            {
                logger.LogInformation("Симуляция успешно завершена.");
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Убеждаемся, что appsettings.json загружается из правильной директории
                    var env = context.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    // Регистрация базы данных
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<SimulationDbContext>(options =>
                        options.UseNpgsql(connectionString));

                    // Регистрация сервиса сохранения состояния
                    services.AddScoped<ISimulationStateService, SimulationStateService>();

                    // Регистрация конфигурации
                    services.Configure<SimulationOptions>(configuration.GetSection(SimulationOptions.SectionName));
                    services.Configure<PhilosophersOptions>(configuration.GetSection(PhilosophersOptions.SectionName));

                    // Получение опций для инициализации
                    var simulationSection = configuration.GetSection(SimulationOptions.SectionName);
                    if (!simulationSection.Exists())
                    {
                        throw new InvalidOperationException($"Секция конфигурации '{SimulationOptions.SectionName}' не найдена. Проверьте наличие appsettings.json");
                    }
                    
                    var simulationOptions = simulationSection.Get<SimulationOptions>();
                    if (simulationOptions == null)
                    {
                        throw new InvalidOperationException($"Не удалось десериализовать {nameof(SimulationOptions)} из конфигурации");
                    }
                    
                    var philosophersSection = configuration.GetSection(PhilosophersOptions.SectionName);
                    if (!philosophersSection.Exists())
                    {
                        throw new InvalidOperationException($"Секция конфигурации '{PhilosophersOptions.SectionName}' не найдена. Проверьте наличие appsettings.json");
                    }
                    
                    var philosophersOptions = philosophersSection.Get<PhilosophersOptions>();
                    if (philosophersOptions == null)
                    {
                        throw new InvalidOperationException($"Не удалось десериализовать {nameof(PhilosophersOptions)} из конфигурации");
                    }

                    // Регистрация сервисов
                    // Объекты
                    var forkManager = new ForkManager(simulationOptions.TotalForks);
                    services.AddSingleton<IForkManager>(forkManager);
                    var metricsCalculator = new MetricsCalculator();
                    metricsCalculator.StartSimulation(forkManager.GetAllForks());
                    services.AddSingleton<IMetricsCalculator>(metricsCalculator);
                    services.AddSingleton<IDisplayService, DisplayService>();
                    IPhilosopherStrategy strategy = simulationOptions.Strategy.ToLowerInvariant() switch
                    {
                        "naive" => new NaiveStrategy(),
                        _ => new NaiveStrategy()
                    };
                    services.AddSingleton<IPhilosopherStrategy>(strategy);
                    var philosophers = CreatePhilosophers(simulationOptions, philosophersOptions, forkManager);
                    services.AddSingleton(philosophers);
                    foreach (var philosopher in philosophers)
                    {
                        var philosopherCopy = philosopher; // Захват для захвата лямбда-функцией
                        services.AddSingleton<IHostedService>(sp => 
                            PhilosopherHostedServiceFactory.Create(sp, philosopherCopy));
                    }

                    // Сервисы
                    services.AddHostedService<DisplayHostedService>();
                    services.AddHostedService<SimulationLifecycleService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });

        private static Philosopher[] CreatePhilosophers(
            SimulationOptions simulationOptions,
            PhilosophersOptions philosophersOptions,
            IForkManager forkManager)
        {
            var philosophers = new Philosopher[simulationOptions.TotalPhilosophers];
            var forks = forkManager.GetAllForks();

            PhilosopherContext? defaultContext = null;
            if (philosophersOptions.DefaultContext != null)
            {
                defaultContext = new PhilosopherContext(philosophersOptions.DefaultContext);
            }

            for (int i = 0; i < simulationOptions.TotalPhilosophers; i++)
            {
                string name = philosophersOptions.Names?[i] ?? philosophersOptions.Philosophers?[i].Name ?? $"Philosopher{i+1}";

                PhilosopherContext pCtx;
                if (defaultContext != null)
                {
                    pCtx = defaultContext;
                }
                else if (philosophersOptions.Philosophers != null && i < philosophersOptions.Philosophers.Length)
                {
                    pCtx = new PhilosopherContext(philosophersOptions.Philosophers[i].Context);
                }
                else
                {
                    throw new InvalidOperationException(
                        "Ни Philosopher[].Context, ни DefaultContext не заданы");
                }

                Fork leftFork = forks[i];
                Fork rightFork = forks[(i + 1) % simulationOptions.TotalForks];

                philosophers[i] = new Philosopher(name, leftFork, rightFork, pCtx);
            }

            return philosophers;
        }
    }
}
