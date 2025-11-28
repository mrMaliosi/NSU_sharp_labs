using Xunit;
using Lab1.DiningPhilosophers;

namespace DiningPhilosophers.Tests
{
    /// <summary>
    /// Тесты для ForkManager
    /// </summary>
    public class ForkManagerTests
    {
        [Fact]
        public void ForkManager_Create_InitializesCorrectly()
        {
            // Arrange & Act
            var forkManager = new ForkManager(5);

            // Assert
            Assert.Equal(5, forkManager.TotalForks);
            Assert.NotNull(forkManager.GetAllForks());
            Assert.Equal(5, forkManager.GetAllForks().Length);
        }

        [Fact]
        public void ForkManager_GetFork_ReturnsCorrectFork()
        {
            // Arrange
            var forkManager = new ForkManager(5);

            // Act
            var fork0 = forkManager.GetFork(0);
            var fork2 = forkManager.GetFork(2);
            var fork4 = forkManager.GetFork(4);

            // Assert
            Assert.NotNull(fork0);
            Assert.NotNull(fork2);
            Assert.NotNull(fork4);
            Assert.Equal(0, fork0.Id);
            Assert.Equal(2, fork2.Id);
            Assert.Equal(4, fork4.Id);
        }

        [Fact]
        public void ForkManager_GetAllForks_ReturnsAllForks()
        {
            // Arrange
            var forkManager = new ForkManager(3);

            // Act
            var forks = forkManager.GetAllForks();

            // Assert
            Assert.Equal(3, forks.Length);
            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(i, forks[i].Id);
            }
        }

        [Fact]
        public void ForkManager_Forks_AreInitiallyAvailable()
        {
            // Arrange
            var forkManager = new ForkManager(3);

            // Act
            var forks = forkManager.GetAllForks();

            // Assert
            foreach (var fork in forks)
            {
                var (state, heldBy) = fork.GetState();
                Assert.Equal(ForkState.Available, state);
                Assert.Null(heldBy);
            }
        }
    }
}

