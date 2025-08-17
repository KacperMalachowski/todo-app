using todo.app.Models;

namespace todo.Tests;

public class TodoTaskTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var task = new TodoTask();

        // Assert
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(string.Empty, task.Title);
        Assert.False(task.IsCompleted);
        Assert.True(task.CreatedAt <= DateTime.UtcNow);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void Constructor_WithTitle_ShouldSetTitle()
    {
        // Arrange
        const string expectedTitle = "Test Task";

        // Act
        var task = new TodoTask(expectedTitle);

        // Assert
        Assert.Equal(expectedTitle, task.Title);
        Assert.False(task.IsCompleted);
    }

    [Fact]
    public void Constructor_WithNullTitle_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TodoTask(null!));
    }

    [Fact]
    public void MarkAsCompleted_ShouldSetCompletedProperties()
    {
        // Arrange
        var task = new TodoTask("Test Task");
        var beforeCompletion = DateTime.UtcNow;

        // Act
        task.MarkAsCompleted();

        // Assert
        Assert.True(task.IsCompleted);
        Assert.NotNull(task.CompletedAt);
        Assert.True(task.CompletedAt >= beforeCompletion);
    }

    [Fact]
    public void MarkAsCompleted_WhenAlreadyCompleted_ShouldNotChangeCompletedAt()
    {
        // Arrange
        var task = new TodoTask("Test Task");
        task.MarkAsCompleted();
        var originalCompletedAt = task.CompletedAt;

        // Act
        task.MarkAsCompleted();

        // Assert
        Assert.True(task.IsCompleted);
        Assert.Equal(originalCompletedAt, task.CompletedAt);
    }

    [Fact]
    public void MarkAsIncomplete_ShouldResetCompletedProperties()
    {
        // Arrange
        var task = new TodoTask("Test Task");
        task.MarkAsCompleted();

        // Act
        task.MarkAsIncomplete();

        // Assert
        Assert.False(task.IsCompleted);
        Assert.Null(task.CompletedAt);
    }
}
