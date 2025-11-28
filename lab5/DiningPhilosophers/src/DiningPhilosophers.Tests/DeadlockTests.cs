using Xunit;
using Lab1.DiningPhilosophers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DiningPhilosophers.Tests
{
    /// <summary>
    /// Тесты для проверки deadlock ситуаций
    /// </summary>
    public class DeadlockTests
    {
        [Fact]
        public void Deadlock_NaiveStrategy_CanOccur_WithMultiplePhilosophers()
        {
            // Arrange - создаем ситуацию, где все философы могут взять по одной вилке
            var strategy = new NaiveStrategy();
            var context = CreateTestContext(thinkingTime: new Segment(1, 1));
            var forks = new Fork[5];
            var philosophers = new Philosopher[5];

            for (int i = 0; i < 5; i++)
            {
                forks[i] = new Fork(i);
            }

            for (int i = 0; i < 5; i++)
            {
                var leftFork = forks[i];
                var rightFork = forks[(i + 1) % 5];
                philosophers[i] = new Philosopher($"Philosopher{i}", leftFork, rightFork, context);
            }

            // Act - переводим всех в состояние Hungry и пытаемся взять вилки
            Thread.Sleep(10);
            foreach (var philosopher in philosophers)
            {
                philosopher.RealiseThePassageOfTime();
            }

            // Симулируем ситуацию deadlock: каждый философ берет левую вилку
            bool allLeftForksTaken = true;
            for (int i = 0; i < 5; i++)
            {
                var picked = philosophers[i].PickLeftFork();
                if (!picked)
                {
                    allLeftForksTaken = false;
                    break;
                }
            }

            // Проверяем, что правые вилки недоступны (deadlock)
            bool allRightForksUnavailable = true;
            for (int i = 0; i < 5; i++)
            {
                var picked = philosophers[i].PickRightFork();
                if (picked)
                {
                    allRightForksUnavailable = false;
                    break;
                }
            }

            // Assert - если все левые вилки взяты, правые должны быть недоступны (deadlock)
            if (allLeftForksTaken)
            {
                Assert.True(allRightForksUnavailable, "Deadlock ситуация: все левые вилки взяты, правые недоступны");
                
                // Все философы должны остаться в состоянии Hungry
                foreach (var philosopher in philosophers)
                {
                    Assert.Equal(PhilosopherState.Hungry, philosopher.State);
                }
            }
        }

        [Fact]
        public void Deadlock_ResourceHierarchyStrategy_PreventsDeadlock()
        {
            // Arrange - создаем ситуацию с 5 философами
            var strategy = new ResourceHierarchyStrategy();
            var context = CreateTestContext(thinkingTime: new Segment(1, 1), eatingTime: new Segment(1, 1));
            var forks = new Fork[5];
            var philosophers = new Philosopher[5];

            for (int i = 0; i < 5; i++)
            {
                forks[i] = new Fork(i);
            }

            for (int i = 0; i < 5; i++)
            {
                var leftFork = forks[i];
                var rightFork = forks[(i + 1) % 5];
                philosophers[i] = new Philosopher($"Philosopher{i}", leftFork, rightFork, context);
            }

            // Act - переводим всех в состояние Hungry и применяем стратегию
            Thread.Sleep(10);
            foreach (var philosopher in philosophers)
            {
                philosopher.RealiseThePassageOfTime();
            }

            // Применяем стратегию несколько раз
            for (int iteration = 0; iteration < 50; iteration++)
            {
                foreach (var philosopher in philosophers)
                {
                    strategy.PerformAction(philosopher, philosophers, forks);
                }
                Thread.Sleep(1);
            }

            // Assert - хотя бы один философ должен был поесть (deadlock предотвращен)
            int mealsEaten = philosophers.Sum(p => p.MealsEaten);
            Assert.True(mealsEaten > 0, "Стратегия иерархии ресурсов должна предотвращать deadlock");
        }

        [Fact]
        public void Deadlock_Detection_AllPhilosophersHungry_NoProgress()
        {
            // Arrange
            var context = CreateTestContext(thinkingTime: new Segment(1, 1));
            var forks = new Fork[3];
            var philosophers = new Philosopher[3];

            for (int i = 0; i < 3; i++)
            {
                forks[i] = new Fork(i);
            }

            for (int i = 0; i < 3; i++)
            {
                var leftFork = forks[i];
                var rightFork = forks[(i + 1) % 3];
                philosophers[i] = new Philosopher($"Philosopher{i}", leftFork, rightFork, context);
            }

            // Act - создаем deadlock ситуацию
            Thread.Sleep(10);
            foreach (var philosopher in philosophers)
            {
                philosopher.RealiseThePassageOfTime();
            }

            // Каждый берет левую вилку
            foreach (var philosopher in philosophers)
            {
                philosopher.PickLeftFork();
            }

            // Проверяем состояние через несколько итераций
            int initialMeals = philosophers.Sum(p => p.MealsEaten);
            for (int i = 0; i < 10; i++)
            {
                foreach (var philosopher in philosophers)
                {
                    philosopher.RealiseThePassageOfTime();
                }
                Thread.Sleep(1);
            }
            int finalMeals = philosophers.Sum(p => p.MealsEaten);

            // Assert - в deadlock нет прогресса
            Assert.Equal(initialMeals, finalMeals);
            Assert.All(philosophers, p => Assert.Equal(PhilosopherState.Hungry, p.State));
        }

        [Fact]
        public void Deadlock_Resolution_WhenOnePhilosopherReleasesForks()
        {
            // Arrange
            var context = CreateTestContext(thinkingTime: new Segment(1, 1), eatingTime: new Segment(1, 1));
            var forks = new Fork[3];
            var philosophers = new Philosopher[3];

            for (int i = 0; i < 3; i++)
            {
                forks[i] = new Fork(i);
            }

            for (int i = 0; i < 3; i++)
            {
                var leftFork = forks[i];
                var rightFork = forks[(i + 1) % 3];
                philosophers[i] = new Philosopher($"Philosopher{i}", leftFork, rightFork, context);
            }

            // Act - создаем deadlock
            Thread.Sleep(10);
            foreach (var philosopher in philosophers)
            {
                philosopher.RealiseThePassageOfTime();
            }

            foreach (var philosopher in philosophers)
            {
                philosopher.PickLeftFork();
            }

            // Один философ отпускает вилки (разрешение deadlock)
            philosophers[0].RealizeFutilityOfBeing();

            // Теперь другие могут взять вилки
            bool philosopher1CanEat = philosophers[1].PickRightFork();
            bool philosopher2CanEat = philosophers[2].PickRightFork();

            if (philosopher1CanEat && philosopher2CanEat)
            {
                Thread.Sleep(10);
                philosophers[1].RealiseThePassageOfTime();
                philosophers[2].RealiseThePassageOfTime();
            }

            // Assert - deadlock разрешен, философы могут есть
            Assert.True(philosopher1CanEat || philosopher2CanEat, "Deadlock должен быть разрешен после освобождения вилок");
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

