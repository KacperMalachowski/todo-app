using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

    [Fact]
    public void EditTask_WithValidIdAndTitle_ShouldUpdateTask()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Original Title");
        const string newTitle = "Updated Title";

        // Act
        var result = service.EditTask(task.Id, newTitle);

        // Assert
        Assert.True(result);
        Assert.Equal(newTitle, task.Title);
        var retrievedTask = service.GetTask(task.Id);
        Assert.NotNull(retrievedTask);
        Assert.Equal(newTitle, retrievedTask.Title);
    }

    [Fact]
    public void EditTask_WithWhitespaceTitle_ShouldTrimAndUpdateTask()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Original Title");
        const string newTitle = "  Updated Title  ";
        const string expectedTitle = "Updated Title";

        // Act
        var result = service.EditTask(task.Id, newTitle);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedTitle, task.Title);
    }

    [Fact]
    public void EditTask_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var service = new TodoService();
        var invalidId = Guid.NewGuid();

        // Act
        var result = service.EditTask(invalidId, "New Title");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EditTask_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Original Title");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.EditTask(task.Id, invalidTitle!));

        // Verify original title is unchanged
        Assert.Equal("Original Title", task.Title);
    }

    [Fact]
    public void EditTask_ShouldNotAffectTaskCompletionStatus()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Original Title");
        task.MarkAsCompleted();
        var originalCompletedAt = task.CompletedAt;

        // Act
        var result = service.EditTask(task.Id, "Updated Title");

        // Assert
        Assert.True(result);
        Assert.Equal("Updated Title", task.Title);
        Assert.True(task.IsCompleted);
        Assert.Equal(originalCompletedAt, task.CompletedAt);
    }

    // Persistence Tests
    private string GetTestFilePath()
    {
        return Path.Combine(Path.GetTempPath(), $"test_service_{Guid.NewGuid()}.json");
    }

    [Fact]
    public async Task LoadTasksAsync_ShouldLoadTasksFromPersistence()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var persistenceService = new DataPersistenceService(testFile);
        var service = new TodoService(persistenceService);

        try
        {
            // Add some tasks directly to persistence
            var tasks = new List<todo.app.Models.TodoTask>
            {
                new("Task 1"),
                new("Task 2")
            };
            await persistenceService.SaveTasksAsync(tasks);

            // Act
            await service.LoadTasksAsync();

            // Assert
            Assert.Equal(2, service.TotalTaskCount);
            Assert.Contains(service.Tasks, t => t.Title == "Task 1");
            Assert.Contains(service.Tasks, t => t.Title == "Task 2");
        }
        finally
        {
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task AddTaskAsync_ShouldAddTaskAndSave()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var persistenceService = new DataPersistenceService(testFile);
        var service = new TodoService(persistenceService);

        try
        {
            // Act
            var task = await service.AddTaskAsync("Test Task");

            // Assert
            Assert.Equal("Test Task", task.Title);
            Assert.Single(service.Tasks);

            // Verify persistence
            var loadedTasks = await persistenceService.LoadTasksAsync();
            Assert.Single(loadedTasks);
            Assert.Equal("Test Task", loadedTasks[0].Title);
        }
        finally
        {
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task RemoveTaskAsync_ShouldRemoveTaskAndSave()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var persistenceService = new DataPersistenceService(testFile);
        var service = new TodoService(persistenceService);
        var task = await service.AddTaskAsync("Test Task");

        try
        {
            // Act
            var result = await service.RemoveTaskAsync(task.Id);

            // Assert
            Assert.True(result);
            Assert.Empty(service.Tasks);

            // Verify persistence
            var loadedTasks = await persistenceService.LoadTasksAsync();
            Assert.Empty(loadedTasks);
        }
        finally
        {
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task ToggleTaskCompletionAsync_ShouldToggleAndSave()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var persistenceService = new DataPersistenceService(testFile);
        var service = new TodoService(persistenceService);
        var task = await service.AddTaskAsync("Test Task");

        try
        {
            // Act
            var result = await service.ToggleTaskCompletionAsync(task.Id);

            // Assert
            Assert.True(result);
            Assert.True(task.IsCompleted);

            // Verify persistence
            var loadedTasks = await persistenceService.LoadTasksAsync();
            Assert.Single(loadedTasks);
            Assert.True(loadedTasks[0].IsCompleted);
        }
        finally
        {
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task EditTaskAsync_ShouldEditTaskAndSave()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var persistenceService = new DataPersistenceService(testFile);
        var service = new TodoService(persistenceService);
        var task = await service.AddTaskAsync("Original Title");

        try
        {
            // Act
            var result = await service.EditTaskAsync(task.Id, "Updated Title");

            // Assert
            Assert.True(result);
            Assert.Equal("Updated Title", task.Title);

            // Verify persistence
            var loadedTasks = await persistenceService.LoadTasksAsync();
            Assert.Single(loadedTasks);
            Assert.Equal("Updated Title", loadedTasks[0].Title);
        }
        finally
        {
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task AsyncMethods_WithNonExistentTask_ShouldReturnFalseAndNotSave()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var persistenceService = new DataPersistenceService(testFile);
        var service = new TodoService(persistenceService);
        var invalidId = Guid.NewGuid();

        try
        {
            // Act & Assert
            Assert.False(await service.RemoveTaskAsync(invalidId));
            Assert.False(await service.ToggleTaskCompletionAsync(invalidId));
            Assert.False(await service.EditTaskAsync(invalidId, "New Title"));

            // Verify no file was created
            Assert.False(File.Exists(testFile));
        }
        finally
        {
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    // Priority Tests
    [Fact]
    public void AddTask_WithPriority_ShouldAddTaskWithCorrectPriority()
    {
        // Arrange
        var service = new TodoService();
        const string taskTitle = "High Priority Task";
        const todo.app.Models.TaskPriority priority = todo.app.Models.TaskPriority.High;

        // Act
        var task = service.AddTask(taskTitle, priority);

        // Assert
        Assert.NotNull(task);
        Assert.Equal(taskTitle, task.Title);
        Assert.Equal(priority, task.Priority);
        Assert.Single(service.Tasks);
    }

    [Fact]
    public async Task AddTaskAsync_WithPriority_ShouldAddTaskWithPriorityAndSave()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var persistenceService = new DataPersistenceService(testFile);
        var service = new TodoService(persistenceService);
        const string taskTitle = "Low Priority Task";
        const todo.app.Models.TaskPriority priority = todo.app.Models.TaskPriority.Low;

        try
        {
            // Act
            var task = await service.AddTaskAsync(taskTitle, priority);

            // Assert
            Assert.Equal(taskTitle, task.Title);
            Assert.Equal(priority, task.Priority);

            // Verify persistence
            var loadedTasks = await persistenceService.LoadTasksAsync();
            Assert.Single(loadedTasks);
            Assert.Equal(taskTitle, loadedTasks[0].Title);
            Assert.Equal(priority, loadedTasks[0].Priority);
        }
        finally
        {
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public void UpdateTaskPriority_WithValidId_ShouldUpdatePriority()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Test Task");
        const todo.app.Models.TaskPriority newPriority = todo.app.Models.TaskPriority.High;

        // Act
        var result = service.UpdateTaskPriority(task.Id, newPriority);

        // Assert
        Assert.True(result);
        Assert.Equal(newPriority, task.Priority);
    }

    [Fact]
    public void UpdateTaskPriority_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var service = new TodoService();
        var invalidId = Guid.NewGuid();

        // Act
        var result = service.UpdateTaskPriority(invalidId, todo.app.Models.TaskPriority.High);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateTaskPriorityAsync_ShouldUpdateAndSave()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var persistenceService = new DataPersistenceService(testFile);
        var service = new TodoService(persistenceService);
        var task = await service.AddTaskAsync("Test Task");
        const todo.app.Models.TaskPriority newPriority = todo.app.Models.TaskPriority.Low;

        try
        {
            // Act
            var result = await service.UpdateTaskPriorityAsync(task.Id, newPriority);

            // Assert
            Assert.True(result);
            Assert.Equal(newPriority, task.Priority);

            // Verify persistence
            var loadedTasks = await persistenceService.LoadTasksAsync();
            Assert.Single(loadedTasks);
            Assert.Equal(newPriority, loadedTasks[0].Priority);
        }
        finally
        {
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Theory]
    [InlineData(todo.app.Models.TaskPriority.Low)]
    [InlineData(todo.app.Models.TaskPriority.Medium)]
    [InlineData(todo.app.Models.TaskPriority.High)]
    public void GetTasksByPriority_ShouldReturnTasksWithSpecifiedPriority(todo.app.Models.TaskPriority priority)
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Low Task", todo.app.Models.TaskPriority.Low);
        service.AddTask("Medium Task", todo.app.Models.TaskPriority.Medium);
        service.AddTask("High Task", todo.app.Models.TaskPriority.High);
        service.AddTask("Another Medium Task", todo.app.Models.TaskPriority.Medium);

        // Act
        var filteredTasks = service.GetTasksByPriority(priority);

        // Assert
        Assert.All(filteredTasks, task => Assert.Equal(priority, task.Priority));
        
        if (priority == todo.app.Models.TaskPriority.Medium)
        {
            Assert.Equal(2, filteredTasks.Count);
        }
        else
        {
            Assert.Single(filteredTasks);
        }
    }

    [Fact]
    public void GetHighPriorityTasks_ShouldReturnOnlyHighPriorityTasks()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Low Task", todo.app.Models.TaskPriority.Low);
        service.AddTask("High Task 1", todo.app.Models.TaskPriority.High);
        service.AddTask("Medium Task", todo.app.Models.TaskPriority.Medium);
        service.AddTask("High Task 2", todo.app.Models.TaskPriority.High);

        // Act
        var highPriorityTasks = service.GetHighPriorityTasks();

        // Assert
        Assert.Equal(2, highPriorityTasks.Count);
        Assert.All(highPriorityTasks, task => Assert.Equal(todo.app.Models.TaskPriority.High, task.Priority));
        Assert.Contains(highPriorityTasks, t => t.Title == "High Task 1");
        Assert.Contains(highPriorityTasks, t => t.Title == "High Task 2");
    }

    [Fact]
    public void GetMediumPriorityTasks_ShouldReturnOnlyMediumPriorityTasks()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Low Task", todo.app.Models.TaskPriority.Low);
        service.AddTask("Medium Task 1", todo.app.Models.TaskPriority.Medium);
        service.AddTask("High Task", todo.app.Models.TaskPriority.High);
        service.AddTask("Medium Task 2", todo.app.Models.TaskPriority.Medium);

        // Act
        var mediumPriorityTasks = service.GetMediumPriorityTasks();

        // Assert
        Assert.Equal(2, mediumPriorityTasks.Count);
        Assert.All(mediumPriorityTasks, task => Assert.Equal(todo.app.Models.TaskPriority.Medium, task.Priority));
    }

    [Fact]
    public void GetLowPriorityTasks_ShouldReturnOnlyLowPriorityTasks()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Low Task 1", todo.app.Models.TaskPriority.Low);
        service.AddTask("Medium Task", todo.app.Models.TaskPriority.Medium);
        service.AddTask("High Task", todo.app.Models.TaskPriority.High);
        service.AddTask("Low Task 2", todo.app.Models.TaskPriority.Low);

        // Act
        var lowPriorityTasks = service.GetLowPriorityTasks();

        // Assert
        Assert.Equal(2, lowPriorityTasks.Count);
        Assert.All(lowPriorityTasks, task => Assert.Equal(todo.app.Models.TaskPriority.Low, task.Priority));
    }
}
