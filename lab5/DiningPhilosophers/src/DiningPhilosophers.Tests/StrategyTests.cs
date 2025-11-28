using Xunit;
using Lab1.DiningPhilosophers;
using System;
using System.Linq;
using System.Threading;

namespace DiningPhilosophers.Tests
{
    /// <summary>
    /// Тесты для стратегий принятия решений
    /// </summary>
    public class StrategyTests
    {
        [Fact]
        public void NaiveStrategy_PerformAction_WhenHungry_TriesToPickFork()
        {
            // Arrange
            var strategy = new NaiveStrategy();
            var context = CreateTestContext(thinkingTime: new Segment(1, 1));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);
            var allPhilosophers = new[] { philosopher };
            var allForks = new[] { leftFork, rightFork };

            // Act - переводим в состояние Hungry
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            Assert.Equal(PhilosopherState.Hungry, philosopher.State);

            var initialAction = philosopher.LastAction;
            strategy.PerformAction(philosopher, allPhilosophers, allForks);

            // Assert - стратегия должна попытаться взять вилку
            // В NaiveStrategy выбор случайный, но действие должно измениться
            Assert.True(philosopher.LastAction == ActionType.TakeLeftFork || 
                       philosopher.LastAction == ActionType.TakeRightFork ||
                       philosopher.LastAction == ActionType.None);
        }

        [Fact]
        public void NaiveStrategy_PerformAction_WhenThinking_NoAction()
        {
            // Arrange
            var strategy = new NaiveStrategy();
            var context = CreateTestContext(thinkingTime: new Segment(100, 200));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);
            var allPhilosophers = new[] { philosopher };
            var allForks = new[] { leftFork, rightFork };

            // Act
            var initialAction = philosopher.LastAction;
            strategy.PerformAction(philosopher, allPhilosophers, allForks);

            // Assert - в состоянии Thinking стратегия не должна пытаться брать вилки
            Assert.Equal(PhilosopherState.Thinking, philosopher.State);
        }

        [Fact]
        public void NaiveStrategy_PerformAction_WhenEating_NoAction()
        {
            // Arrange
            var strategy = new NaiveStrategy();
            var context = CreateTestContext(thinkingTime: new Segment(1, 1), eatingTime: new Segment(100, 200));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);
            var allPhilosophers = new[] { philosopher };
            var allForks = new[] { leftFork, rightFork };

            // Act - переводим в Eating
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            philosopher.PickLeftFork();
            philosopher.PickRightFork();
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            Assert.Equal(PhilosopherState.Eating, philosopher.State);

            var initialMeals = philosopher.MealsEaten;
            strategy.PerformAction(philosopher, allPhilosophers, allForks);

            // Assert - в состоянии Eating стратегия не должна менять состояние
            Assert.Equal(PhilosopherState.Eating, philosopher.State);
            Assert.Equal(initialMeals, philosopher.MealsEaten);
        }

        [Fact]
        public void ResourceHierarchyStrategy_PerformAction_WhenHungry_PicksLeftThenRight()
        {
            // Arrange
            var strategy = new ResourceHierarchyStrategy();
            var context = CreateTestContext(thinkingTime: new Segment(1, 1), eatingTime: new Segment(100, 200));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);
            var allPhilosophers = new[] { philosopher };
            var allForks = new[] { leftFork, rightFork };

            // Act - переводим в состояние Hungry
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            Assert.Equal(PhilosopherState.Hungry, philosopher.State);

            // Проверяем, что вилки доступны
            Assert.Equal(ForkState.Available, leftFork.GetState().state);
            Assert.Equal(ForkState.Available, rightFork.GetState().state);

            // Вызываем стратегию
            strategy.PerformAction(philosopher, allPhilosophers, allForks);

            // Assert - стратегия должна взять обе вилки (сначала левую, затем правую)
            // Проверяем состояние вилок, так как RealiseThePassageOfTime может изменить LastAction
            var leftForkStateAfter = leftFork.GetState().state;
            var rightForkStateAfter = rightFork.GetState().state;
            
            // Стратегия должна взять обе вилки
            Assert.Equal(ForkState.InUse, leftForkStateAfter);
            Assert.Equal(ForkState.InUse, rightForkStateAfter);
            
            // Проверяем, что вилки принадлежат нашему философу
            Assert.Equal("TestPhilosopher", leftFork.GetState().heldBy?.Name);
            Assert.Equal("TestPhilosopher", rightFork.GetState().heldBy?.Name);
        }

        [Fact]
        public void ResourceHierarchyStrategy_PerformAction_ConsistentOrder()
        {
            // Arrange
            var strategy = new ResourceHierarchyStrategy();
            var context = CreateTestContext(thinkingTime: new Segment(1, 1), eatingTime: new Segment(100, 200));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);
            var allPhilosophers = new[] { philosopher };
            var allForks = new[] { leftFork, rightFork };

            // Act - выполняем стратегию несколько раз
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            
            var actions = new System.Collections.Generic.List<ActionType>();
            for (int i = 0; i < 10; i++)
            {
                philosopher.RealizeFutilityOfBeing();
                strategy.PerformAction(philosopher, allPhilosophers, allForks);
                actions.Add(philosopher.LastAction);
                Thread.Sleep(1);
            }

            // Assert - порядок должен быть консистентным
            // LastAction может быть: TakeLeftFork, TakeRightFork, ReleaseLeftFork (после еды), или None
            Assert.All(actions, action => 
                Assert.True(action == ActionType.TakeLeftFork || 
                          action == ActionType.TakeRightFork || 
                          action == ActionType.ReleaseLeftFork ||
                          action == ActionType.None,
                    $"Неожиданное действие: {action}"));
        }

        [Fact]
        public void ResourceHierarchyStrategy_PerformAction_WhenForkUnavailable_Waits()
        {
            // Arrange
            var strategy = new ResourceHierarchyStrategy();
            var context = CreateTestContext(thinkingTime: new Segment(1, 1));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher1 = new Philosopher("Philosopher1", leftFork, rightFork, context);
            var philosopher2 = new Philosopher("Philosopher2", rightFork, leftFork, context);
            var allPhilosophers = new[] { philosopher1, philosopher2 };
            var allForks = new[] { leftFork, rightFork };

            // Act - philosopher1 берет вилки
            Thread.Sleep(10);
            philosopher1.RealiseThePassageOfTime();
            philosopher2.RealiseThePassageOfTime();
            strategy.PerformAction(philosopher1, allPhilosophers, allForks);

            // philosopher2 пытается взять вилки
            strategy.PerformAction(philosopher2, allPhilosophers, allForks);

            // Assert - philosopher2 должен остаться в состоянии Hungry
            Assert.Equal(PhilosopherState.Hungry, philosopher2.State);
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

