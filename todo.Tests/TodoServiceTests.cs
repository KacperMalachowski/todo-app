using System;
using System.Linq;
using todo.app.Services;
using Xunit;

namespace todo.Tests;

public class TodoServiceTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyTaskList()
    {
        // Act
        var service = new TodoService();

        // Assert
        Assert.Empty(service.Tasks);
        Assert.Equal(0, service.TotalTaskCount);
        Assert.Equal(0, service.CompletedTaskCount);
        Assert.Equal(0, service.PendingTaskCount);
    }

    [Fact]
    public void AddTask_WithValidTitle_ShouldCreateAndAddTask()
    {
        // Arrange
        var service = new TodoService();
        const string taskTitle = "Test Task";

        // Act
        var task = service.AddTask(taskTitle);

        // Assert
        Assert.NotNull(task);
        Assert.Equal(taskTitle, task.Title);
        Assert.False(task.IsCompleted);
        Assert.Single(service.Tasks);
        Assert.Equal(1, service.TotalTaskCount);
        Assert.Equal(0, service.CompletedTaskCount);
        Assert.Equal(1, service.PendingTaskCount);
    }

    [Fact]
    public void AddTask_WithWhitespaceTitle_ShouldTrimAndAddTask()
    {
        // Arrange
        var service = new TodoService();
        const string taskTitle = "  Test Task  ";
        const string expectedTitle = "Test Task";

        // Act
        var task = service.AddTask(taskTitle);

        // Assert
        Assert.Equal(expectedTitle, task.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void AddTask_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Arrange
        var service = new TodoService();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.AddTask(invalidTitle!));
    }

    [Fact]
    public void RemoveTask_WithExistingTask_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Test Task");

        // Act
        var result = service.RemoveTask(task.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(service.Tasks);
        Assert.Equal(0, service.TotalTaskCount);
    }

    [Fact]
    public void RemoveTask_WithNonExistingTask_ShouldReturnFalse()
    {
        // Arrange
        var service = new TodoService();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = service.RemoveTask(nonExistentId);

        // Assert
        Assert.False(result);
        Assert.Empty(service.Tasks);
    }

    [Fact]
    public void GetTask_WithExistingTask_ShouldReturnTask()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Test Task");

        // Act
        var foundTask = service.GetTask(task.Id);

        // Assert
        Assert.NotNull(foundTask);
        Assert.Equal(task.Id, foundTask.Id);
        Assert.Equal(task.Title, foundTask.Title);
    }

    [Fact]
    public void GetTask_WithNonExistingTask_ShouldReturnNull()
    {
        // Arrange
        var service = new TodoService();
        var nonExistentId = Guid.NewGuid();

        // Act
        var foundTask = service.GetTask(nonExistentId);

        // Assert
        Assert.Null(foundTask);
    }

    [Fact]
    public void ToggleTaskCompletion_WithExistingIncompleteTask_ShouldMarkAsCompleted()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Test Task");
        Assert.False(task.IsCompleted); // Ensure it starts incomplete

        // Act
        var result = service.ToggleTaskCompletion(task.Id);

        // Assert
        Assert.True(result);
        Assert.True(task.IsCompleted);
        Assert.Equal(1, service.CompletedTaskCount);
        Assert.Equal(0, service.PendingTaskCount);
    }

    [Fact]
    public void ToggleTaskCompletion_WithExistingCompletedTask_ShouldMarkAsIncomplete()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Test Task");
        task.MarkAsCompleted(); // Mark as completed first

        // Act
        var result = service.ToggleTaskCompletion(task.Id);

        // Assert
        Assert.True(result);
        Assert.False(task.IsCompleted);
        Assert.Equal(0, service.CompletedTaskCount);
        Assert.Equal(1, service.PendingTaskCount);
    }

    [Fact]
    public void ToggleTaskCompletion_WithNonExistingTask_ShouldReturnFalse()
    {
        // Arrange
        var service = new TodoService();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = service.ToggleTaskCompletion(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ClearAllTasks_ShouldRemoveAllTasks()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Task 1");
        service.AddTask("Task 2");
        service.AddTask("Task 3");

        // Act
        service.ClearAllTasks();

        // Assert
        Assert.Empty(service.Tasks);
        Assert.Equal(0, service.TotalTaskCount);
        Assert.Equal(0, service.CompletedTaskCount);
        Assert.Equal(0, service.PendingTaskCount);
    }

    [Fact]
    public void GetCompletedTasks_ShouldReturnOnlyCompletedTasks()
    {
        // Arrange
        var service = new TodoService();
        var task1 = service.AddTask("Task 1");
        var task2 = service.AddTask("Task 2");
        var task3 = service.AddTask("Task 3");

        // Mark some tasks as completed
        task1.MarkAsCompleted();
        task3.MarkAsCompleted();

        // Act
        var completedTasks = service.GetCompletedTasks();

        // Assert
        Assert.Equal(2, completedTasks.Count);
        Assert.Contains(task1, completedTasks);
        Assert.Contains(task3, completedTasks);
        Assert.DoesNotContain(task2, completedTasks);
    }

    [Fact]
    public void GetPendingTasks_ShouldReturnOnlyIncompleteTasks()
    {
        // Arrange
        var service = new TodoService();
        var task1 = service.AddTask("Task 1");
        var task2 = service.AddTask("Task 2");
        var task3 = service.AddTask("Task 3");

        // Mark some tasks as completed
        task1.MarkAsCompleted();
        task3.MarkAsCompleted();

        // Act
        var pendingTasks = service.GetPendingTasks();

        // Assert
        Assert.Single(pendingTasks);
        Assert.Contains(task2, pendingTasks);
        Assert.DoesNotContain(task1, pendingTasks);
        Assert.DoesNotContain(task3, pendingTasks);
    }

    [Fact]
    public void TaskCountProperties_ShouldUpdateCorrectly()
    {
        // Arrange
        var service = new TodoService();

        // Act & Assert - Initial state
        Assert.Equal(0, service.TotalTaskCount);
        Assert.Equal(0, service.CompletedTaskCount);
        Assert.Equal(0, service.PendingTaskCount);

        // Add tasks
        var task1 = service.AddTask("Task 1");
        var task2 = service.AddTask("Task 2");
        var task3 = service.AddTask("Task 3");

        Assert.Equal(3, service.TotalTaskCount);
        Assert.Equal(0, service.CompletedTaskCount);
        Assert.Equal(3, service.PendingTaskCount);

        // Complete some tasks
        task1.MarkAsCompleted();
        task2.MarkAsCompleted();

        Assert.Equal(3, service.TotalTaskCount);
        Assert.Equal(2, service.CompletedTaskCount);
        Assert.Equal(1, service.PendingTaskCount);

        // Remove a task
        service.RemoveTask(task3.Id);

        Assert.Equal(2, service.TotalTaskCount);
        Assert.Equal(2, service.CompletedTaskCount);
        Assert.Equal(0, service.PendingTaskCount);
    }
}
