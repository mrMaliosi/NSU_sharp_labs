using Xunit;
using Lab1.DiningPhilosophers;
using System;
using System.Threading;

namespace DiningPhilosophers.Tests
{
    /// <summary>
    /// Тесты для проверки переходов между состояниями философа
    /// </summary>
    public class PhilosopherSimulationTests
    {
        [Fact]
        public void Philosopher_InitialState_ShouldBeThinking()
        {
            // Arrange
            var context = CreateTestContext();
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);

            // Assert
            Assert.Equal(PhilosopherState.Thinking, philosopher.State);
        }

        [Fact]
        public void Philosopher_ThinkingToHungry_Transition()
        {
            // Arrange
            var context = CreateTestContext(thinkingTime: new Segment(1, 1));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);

            // Act - симулируем прохождение времени
            Thread.Sleep(10); // Ждем больше, чем thinkingTime
            philosopher.RealiseThePassageOfTime();

            // Assert
            Assert.Equal(PhilosopherState.Hungry, philosopher.State);
        }

        [Fact]
        public void Philosopher_HungryToEating_WhenBothForksAvailable()
        {
            // Arrange
            var context = CreateTestContext(thinkingTime: new Segment(1, 1), eatingTime: new Segment(100, 100));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);

            // Act - переходим в состояние Hungry
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            Assert.Equal(PhilosopherState.Hungry, philosopher.State);

            // Берем обе вилки
            var leftPicked = philosopher.PickLeftFork();
            var rightPicked = philosopher.PickRightFork();

            // Симулируем прохождение времени
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();

            // Assert
            Assert.True(leftPicked);
            Assert.True(rightPicked);
            Assert.Equal(PhilosopherState.Eating, philosopher.State);
        }

        [Fact]
        public void Philosopher_Hungry_StaysHungry_WhenForksNotAvailable()
        {
            // Arrange
            var context = CreateTestContext(thinkingTime: new Segment(1, 1));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher1 = new Philosopher("Philosopher1", leftFork, rightFork, context);
            var philosopher2 = new Philosopher("Philosopher2", rightFork, leftFork, context);

            // Act - philosopher1 берет обе вилки
            Thread.Sleep(10);
            philosopher1.RealiseThePassageOfTime();
            philosopher1.PickLeftFork();
            philosopher1.PickRightFork();

            // philosopher2 пытается взять вилки, но они заняты
            Thread.Sleep(10);
            philosopher2.RealiseThePassageOfTime();
            var leftPicked = philosopher2.PickLeftFork();
            var rightPicked = philosopher2.PickRightFork();

            Thread.Sleep(10);
            philosopher2.RealiseThePassageOfTime();

            // Assert
            Assert.False(leftPicked || rightPicked);
            Assert.Equal(PhilosopherState.Hungry, philosopher2.State);
        }

        [Fact]
        public void Philosopher_EatingToThinking_AfterEating()
        {
            // Arrange
            var context = CreateTestContext(thinkingTime: new Segment(1, 1), eatingTime: new Segment(1, 1));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);

            // Act - переходим в Eating
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            philosopher.PickLeftFork();
            philosopher.PickRightFork();
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            Assert.Equal(PhilosopherState.Eating, philosopher.State);

            // Ждем окончания еды
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();

            // Assert
            Assert.Equal(PhilosopherState.Thinking, philosopher.State);
            Assert.Equal(1, philosopher.MealsEaten);
        }

        [Fact]
        public void Philosopher_MealEaten_EventFired()
        {
            // Arrange
            var context = CreateTestContext(thinkingTime: new Segment(1, 1), eatingTime: new Segment(1, 1));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);
            bool eventFired = false;
            string? eventPhilosopherName = null;

            philosopher.OnMealEaten += (name) =>
            {
                eventFired = true;
                eventPhilosopherName = name;
            };

            // Act - завершаем цикл еды
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            philosopher.PickLeftFork();
            philosopher.PickRightFork();
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();

            // Assert
            Assert.True(eventFired);
            Assert.Equal("TestPhilosopher", eventPhilosopherName);
        }

        [Fact]
        public void Philosopher_LastAction_UpdatedCorrectly()
        {
            // Arrange
            var context = CreateTestContext();
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);

            // Act
            philosopher.PickLeftFork();
            Assert.Equal(ActionType.TakeLeftFork, philosopher.LastAction);

            philosopher.PickRightFork();
            Assert.Equal(ActionType.TakeRightFork, philosopher.LastAction);
        }

        [Fact]
        public void Philosopher_IsBusy_ReturnsTrue_WhenTimeRemaining()
        {
            // Arrange
            var context = CreateTestContext(thinkingTime: new Segment(100, 100));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);

            // Act
            var isBusy = philosopher.IsBusy();

            // Assert
            Assert.True(isBusy);
        }

        [Fact]
        public void Philosopher_IsBusy_ReturnsFalse_WhenTimeExpired()
        {
            // Arrange
            var context = CreateTestContext(thinkingTime: new Segment(1, 1));
            var leftFork = new Fork(0);
            var rightFork = new Fork(1);
            var philosopher = new Philosopher("TestPhilosopher", leftFork, rightFork, context);

            // Act
            Thread.Sleep(10);
            philosopher.RealiseThePassageOfTime();

            // Assert
            Assert.False(philosopher.IsBusy());
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

