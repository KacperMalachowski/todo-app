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

    [Fact]
    public void UpdateTitle_WithValidTitle_ShouldUpdateTitle()
    {
        // Arrange
        var task = new TodoTask("Original Title");
        const string newTitle = "Updated Title";

        // Act
        task.UpdateTitle(newTitle);

        // Assert
        Assert.Equal(newTitle, task.Title);
    }

    [Fact]
    public void UpdateTitle_WithWhitespaceTitle_ShouldUpdateTitle()
    {
        // Arrange
        var task = new TodoTask("Original Title");
        const string newTitle = "  Updated Title  ";

        // Act
        task.UpdateTitle(newTitle);

        // Assert
        Assert.Equal(newTitle, task.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateTitle_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Arrange
        var task = new TodoTask("Original Title");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => task.UpdateTitle(invalidTitle!));

        // Verify original title is unchanged
        Assert.Equal("Original Title", task.Title);
    }

    [Fact]
    public void UpdateTitle_ShouldNotAffectOtherProperties()
    {
        // Arrange
        var task = new TodoTask("Original Title");
        task.MarkAsCompleted();
        var originalId = task.Id;
        var originalCreatedAt = task.CreatedAt;
        var originalCompletedAt = task.CompletedAt;
        var originalIsCompleted = task.IsCompleted;

        // Act
        task.UpdateTitle("New Title");

        // Assert
        Assert.Equal("New Title", task.Title);
        Assert.Equal(originalId, task.Id);
        Assert.Equal(originalCreatedAt, task.CreatedAt);
        Assert.Equal(originalCompletedAt, task.CompletedAt);
        Assert.Equal(originalIsCompleted, task.IsCompleted);
    }

    // Priority Tests
    [Fact]
    public void Constructor_ShouldSetDefaultMediumPriority()
    {
        // Act
        var task = new TodoTask();

        // Assert
        Assert.Equal(todo.app.Models.TaskPriority.Medium, task.Priority);
    }

    [Fact]
    public void Constructor_WithTitle_ShouldSetDefaultMediumPriority()
    {
        // Act
        var task = new TodoTask("Test Task");

        // Assert
        Assert.Equal("Test Task", task.Title);
        Assert.Equal(todo.app.Models.TaskPriority.Medium, task.Priority);
    }

    [Fact]
    public void Constructor_WithTitleAndPriority_ShouldSetPriority()
    {
        // Arrange
        const string title = "High Priority Task";
        const todo.app.Models.TaskPriority priority = todo.app.Models.TaskPriority.High;

        // Act
        var task = new TodoTask(title, priority);

        // Assert
        Assert.Equal(title, task.Title);
        Assert.Equal(priority, task.Priority);
    }

    [Theory]
    [InlineData(todo.app.Models.TaskPriority.Low)]
    [InlineData(todo.app.Models.TaskPriority.High)]
    public void UpdatePriority_ShouldUpdatePriority(todo.app.Models.TaskPriority newPriority)
    {
        // Arrange
        var task = new TodoTask("Test Task"); // Starts with Medium priority
        var originalPriority = task.Priority;

        // Act
        task.UpdatePriority(newPriority);

        // Assert
        Assert.Equal(newPriority, task.Priority);
        Assert.NotEqual(originalPriority, task.Priority);
    }

    [Fact]
    public void UpdatePriority_WithSamePriority_ShouldStillUpdatePriority()
    {
        // Arrange
        var task = new TodoTask("Test Task");
        var originalPriority = task.Priority; // Medium

        // Act
        task.UpdatePriority(todo.app.Models.TaskPriority.Medium);

        // Assert
        Assert.Equal(todo.app.Models.TaskPriority.Medium, task.Priority);
        Assert.Equal(originalPriority, task.Priority);
    }

    [Fact]
    public void UpdatePriority_ShouldNotAffectOtherProperties()
    {
        // Arrange
        var task = new TodoTask("Test Task");
        task.MarkAsCompleted();
        var originalId = task.Id;
        var originalTitle = task.Title;
        var originalCreatedAt = task.CreatedAt;
        var originalCompletedAt = task.CompletedAt;
        var originalIsCompleted = task.IsCompleted;

        // Act
        task.UpdatePriority(todo.app.Models.TaskPriority.High);

        // Assert
        Assert.Equal(todo.app.Models.TaskPriority.High, task.Priority);
        Assert.Equal(originalId, task.Id);
        Assert.Equal(originalTitle, task.Title);
        Assert.Equal(originalCreatedAt, task.CreatedAt);
        Assert.Equal(originalCompletedAt, task.CompletedAt);
        Assert.Equal(originalIsCompleted, task.IsCompleted);
    }

    // Due Date Tests
    [Fact]
    public void Constructor_ShouldSetNullDueDate()
    {
        // Act
        var task = new TodoTask();

        // Assert
        Assert.Null(task.DueDate);
        Assert.False(task.IsOverdue);
        Assert.False(task.IsDueToday);
    }

    [Fact]
    public void Constructor_WithTitleAndDueDate_ShouldSetDueDate()
    {
        // Arrange
        var dueDate = DateTime.Today.AddDays(1);

        // Act
        var task = new TodoTask("Test Task", dueDate);

        // Assert
        Assert.Equal("Test Task", task.Title);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(todo.app.Models.TaskPriority.Medium, task.Priority);
    }

    [Fact]
    public void Constructor_WithTitlePriorityAndDueDate_ShouldSetAllProperties()
    {
        // Arrange
        var dueDate = DateTime.Today.AddDays(2);
        var priority = todo.app.Models.TaskPriority.High;

        // Act
        var task = new TodoTask("Important Task", priority, dueDate);

        // Assert
        Assert.Equal("Important Task", task.Title);
        Assert.Equal(priority, task.Priority);
        Assert.Equal(dueDate, task.DueDate);
    }

    [Fact]
    public void UpdateDueDate_ShouldUpdateDueDate()
    {
        // Arrange
        var task = new TodoTask("Test Task");
        var dueDate = DateTime.Today.AddDays(3);

        // Act
        task.UpdateDueDate(dueDate);

        // Assert
        Assert.Equal(dueDate, task.DueDate);
    }

    [Fact]
    public void UpdateDueDate_WithNull_ShouldRemoveDueDate()
    {
        // Arrange
        var task = new TodoTask("Test Task", DateTime.Today.AddDays(1));

        // Act
        task.UpdateDueDate(null);

        // Assert
        Assert.Null(task.DueDate);
    }

    [Fact]
    public void IsOverdue_WithPastDueDate_ShouldReturnTrue()
    {
        // Arrange
        var task = new TodoTask("Test Task", DateTime.Today.AddDays(-1));

        // Act & Assert
        Assert.True(task.IsOverdue);
    }

    [Fact]
    public void IsOverdue_WithFutureDueDate_ShouldReturnFalse()
    {
        // Arrange
        var task = new TodoTask("Test Task", DateTime.Today.AddDays(1));

        // Act & Assert
        Assert.False(task.IsOverdue);
    }

    [Fact]
    public void IsOverdue_WithCompletedTask_ShouldReturnFalse()
    {
        // Arrange
        var task = new TodoTask("Test Task", DateTime.Today.AddDays(-1));
        task.MarkAsCompleted();

        // Act & Assert
        Assert.False(task.IsOverdue);
    }

    [Fact]
    public void IsDueToday_WithTodayDueDate_ShouldReturnTrue()
    {
        // Arrange
        var task = new TodoTask("Test Task", DateTime.Today);

        // Act & Assert
        Assert.True(task.IsDueToday);
    }

    [Fact]
    public void IsDueToday_WithTomorrowDueDate_ShouldReturnFalse()
    {
        // Arrange
        var task = new TodoTask("Test Task", DateTime.Today.AddDays(1));

        // Act & Assert
        Assert.False(task.IsDueToday);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(7, true)]
    [InlineData(-1, false)]
    public void IsDueWithin_ShouldReturnCorrectValue(int daysFromToday, bool expected)
    {
        // Arrange
        var task = new TodoTask("Test Task", DateTime.Today.AddDays(daysFromToday));

        // Act & Assert
        Assert.Equal(expected, task.IsDueWithin(7));
    }

    [Fact]
    public void DaysUntilDue_WithFutureDueDate_ShouldReturnPositiveDays()
    {
        // Arrange
        var task = new TodoTask("Test Task", DateTime.Today.AddDays(5));

        // Act & Assert
        Assert.Equal(5, task.DaysUntilDue);
    }

    [Fact]
    public void DaysUntilDue_WithPastDueDate_ShouldReturnNegativeDays()
    {
        // Arrange
        var task = new TodoTask("Test Task", DateTime.Today.AddDays(-3));

        // Act & Assert
        Assert.Equal(-3, task.DaysUntilDue);
    }

    [Fact]
    public void DaysUntilDue_WithNoDueDate_ShouldReturnNull()
    {
        // Arrange
        var task = new TodoTask("Test Task");

        // Act & Assert
        Assert.Null(task.DaysUntilDue);
    }
}
