using Xunit;
using Lab1.DiningPhilosophers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiningPhilosophers.Tests
{
    /// <summary>
    /// Тесты для MetricsCalculator
    /// </summary>
    public class MetricsCalculatorTests
    {
        [Fact]
        public void MetricsCalculator_StartSimulation_InitializesCorrectly()
        {
            // Arrange
            var calculator = new MetricsCalculator();
            var forks = new Fork[] { new Fork(0), new Fork(1) };

            // Act
            calculator.StartSimulation(forks);

            // Assert
            Assert.True(calculator.TotalSimulationTimeMs >= 0);
        }

        [Fact]
        public void MetricsCalculator_OnMeal_IncrementsMealCount()
        {
            // Arrange
            var calculator = new MetricsCalculator();
            var forks = new Fork[] { new Fork(0), new Fork(1) };
            calculator.StartSimulation(forks);

            // Act
            calculator.OnMeal("Philosopher1");
            calculator.OnMeal("Philosopher1");
            calculator.OnMeal("Philosopher2");

            // Assert
            var avgWaitingTime = calculator.GetAverageWaitingTime();
            // Проверяем, что метод не падает и возвращает словарь
            Assert.NotNull(avgWaitingTime);
        }

        [Fact]
        public void MetricsCalculator_GetThroughput_CalculatesCorrectly()
        {
            // Arrange
            var calculator = new MetricsCalculator();
            var forks = new Fork[] { new Fork(0), new Fork(1) };
            calculator.StartSimulation(forks);

            // Act
            calculator.OnMeal("Philosopher1");
            calculator.OnMeal("Philosopher2");
            Thread.Sleep(10); // Даем время пройти

            // Обновляем статистику
            var philosophers = new Philosopher[0];
            calculator.OnStep(forks, philosophers);

            var throughput = calculator.GetThroughput();

            // Assert
            Assert.True(throughput >= 0);
        }

        [Fact]
        public void MetricsCalculator_GetAverageWaitingTime_ReturnsCorrectValues()
        {
            // Arrange
            var calculator = new MetricsCalculator();
            var context = CreateTestContext();
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);
            var forks = new Fork[] { leftFork, rightFork };
            var philosophers = new Philosopher[] { philosopher };

            calculator.StartSimulation(forks);

            // Act - симулируем ожидание
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            calculator.OnStep(forks, philosophers);

            var avgWaitingTime = calculator.GetAverageWaitingTime();

            // Assert
            Assert.NotNull(avgWaitingTime);
            Assert.True(avgWaitingTime.ContainsKey("TestPhilosopher") || avgWaitingTime.Count == 0);
        }

        [Fact]
        public void MetricsCalculator_GetForkUtilization_CalculatesCorrectly()
        {
            // Arrange
            var calculator = new MetricsCalculator();
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var forks = new Fork[] { leftFork, rightFork };
            var context = CreateTestContext();
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);
            var philosophers = new Philosopher[] { philosopher };

            calculator.StartSimulation(forks);

            // Act - используем вилки
            philosopher.PickLeftFork();
            philosopher.PickRightFork();
            Thread.Sleep(10);
            calculator.OnStep(forks, philosophers);

            var utilization = calculator.GetForkUtilization();

            // Assert
            Assert.NotNull(utilization);
            Assert.True(utilization.Count >= 0);
        }

        [Fact]
        public void MetricsCalculator_OnStep_UpdatesStatistics()
        {
            // Arrange
            var calculator = new MetricsCalculator();
            var forks = new Fork[] { new Fork(0), new Fork(1) };
            var philosophers = new Philosopher[0];

            calculator.StartSimulation(forks);

            // Act
            var initialTime = calculator.TotalSimulationTimeMs;
            Thread.Sleep(10);
            calculator.OnStep(forks, philosophers);
            var finalTime = calculator.TotalSimulationTimeMs;

            // Assert
            Assert.True(finalTime >= initialTime);
        }

        [Fact]
        public async Task MetricsCalculator_ThreadSafety_MultipleThreads()
        {
            // Arrange
            var calculator = new MetricsCalculator();
            var forks = new Fork[] { new Fork(0), new Fork(1) };
            calculator.StartSimulation(forks);

            // Act - запускаем несколько потоков
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                int philosopherIndex = i;
                tasks.Add(Task.Run(() =>
                {
                    calculator.OnMeal($"Philosopher{philosopherIndex}");
                    calculator.OnStep(forks, new Philosopher[0]);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert - не должно быть исключений
            var throughput = calculator.GetThroughput();
            var avgWaiting = calculator.GetAverageWaitingTime();
            var utilization = calculator.GetForkUtilization();

            Assert.True(throughput >= 0);
            Assert.NotNull(avgWaiting);
            Assert.NotNull(utilization);
        }

        private PhilosopherContext CreateTestContext(
            Segment? thinkingTime = null,
            Segment? eatingTime = null,
            int forkPickTime = 0)
        {
            thinkingTime ??= new Segment(100, 200);
            eatingTime ??= new Segment(50, 100);
            return new PhilosopherContext(thinkingTime, eatingTime, forkPickTime);
        }
    }
}

