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

    // Due Date Tests
    [Fact]
    public void AddTask_WithDueDate_ShouldAddTaskWithCorrectDueDate()
    {
        // Arrange
        var service = new TodoService();
        const string taskTitle = "Task with Due Date";
        var dueDate = DateTime.Today.AddDays(7);

        // Act
        var task = service.AddTask(taskTitle, dueDate);

        // Assert
        Assert.Equal(taskTitle, task.Title);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(todo.app.Models.TaskPriority.Medium, task.Priority);
        Assert.Single(service.Tasks);
    }

    [Fact]
    public void AddTask_WithPriorityAndDueDate_ShouldAddTaskWithCorrectProperties()
    {
        // Arrange
        var service = new TodoService();
        const string taskTitle = "Important Deadline";
        const todo.app.Models.TaskPriority priority = todo.app.Models.TaskPriority.High;
        var dueDate = DateTime.Today.AddDays(3);

        // Act
        var task = service.AddTask(taskTitle, priority, dueDate);

        // Assert
        Assert.Equal(taskTitle, task.Title);
        Assert.Equal(priority, task.Priority);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Single(service.Tasks);
    }

    [Fact]
    public async Task AddTaskAsync_WithDueDate_ShouldAddTaskAndSave()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var persistenceService = new DataPersistenceService(testFile);
        var service = new TodoService(persistenceService);
        var dueDate = DateTime.Today.AddDays(5);

        try
        {
            // Act
            var task = await service.AddTaskAsync("Test Task", dueDate);

            // Assert
            Assert.Equal(dueDate, task.DueDate);
            Assert.Single(service.Tasks);

            // Verify persistence
            var loadedTasks = await persistenceService.LoadTasksAsync();
            Assert.Single(loadedTasks);
            Assert.Equal(dueDate, loadedTasks[0].DueDate);
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
    public void UpdateTaskDueDate_WithValidId_ShouldUpdateDueDate()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Test Task");
        var newDueDate = DateTime.Today.AddDays(10);

        // Act
        var result = service.UpdateTaskDueDate(task.Id, newDueDate);

        // Assert
        Assert.True(result);
        Assert.Equal(newDueDate, task.DueDate);
    }

    [Fact]
    public void UpdateTaskDueDate_WithNullDueDate_ShouldRemoveDueDate()
    {
        // Arrange
        var service = new TodoService();
        var task = service.AddTask("Test Task", DateTime.Today.AddDays(1));

        // Act
        var result = service.UpdateTaskDueDate(task.Id, null);

        // Assert
        Assert.True(result);
        Assert.Null(task.DueDate);
    }

    [Fact]
    public void UpdateTaskDueDate_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var service = new TodoService();
        var invalidId = Guid.NewGuid();

        // Act
        var result = service.UpdateTaskDueDate(invalidId, DateTime.Today);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetOverdueTasks_ShouldReturnOnlyOverdueTasks()
    {
        // Arrange
        var service = new TodoService();
        var overdueTask = service.AddTask("Overdue Task", DateTime.Today.AddDays(-2));
        var todayTask = service.AddTask("Today Task", DateTime.Today);
        var futureTask = service.AddTask("Future Task", DateTime.Today.AddDays(2));
        var noDateTask = service.AddTask("No Date Task");

        // Act
        var overdueTasks = service.GetOverdueTasks();

        // Assert
        Assert.Single(overdueTasks);
        Assert.Equal(overdueTask.Id, overdueTasks[0].Id);
    }

    [Fact]
    public void GetTasksDueToday_ShouldReturnOnlyTasksDueToday()
    {
        // Arrange
        var service = new TodoService();
        var overdueTask = service.AddTask("Overdue Task", DateTime.Today.AddDays(-1));
        var todayTask1 = service.AddTask("Today Task 1", DateTime.Today);
        var todayTask2 = service.AddTask("Today Task 2", DateTime.Today);
        var futureTask = service.AddTask("Future Task", DateTime.Today.AddDays(1));

        // Act
        var todayTasks = service.GetTasksDueToday();

        // Assert
        Assert.Equal(2, todayTasks.Count);
        Assert.Contains(todayTasks, t => t.Id == todayTask1.Id);
        Assert.Contains(todayTasks, t => t.Id == todayTask2.Id);
    }

    [Fact]
    public void GetTasksDueWithin_ShouldReturnTasksWithinSpecifiedDays()
    {
        // Arrange
        var service = new TodoService();
        var task1 = service.AddTask("Task 1", DateTime.Today.AddDays(1));
        var task2 = service.AddTask("Task 2", DateTime.Today.AddDays(3));
        var task3 = service.AddTask("Task 3", DateTime.Today.AddDays(8));
        var task4 = service.AddTask("Task 4", DateTime.Today.AddDays(-1));

        // Act
        var tasksWithinWeek = service.GetTasksDueWithin(7);

        // Assert
        Assert.Equal(2, tasksWithinWeek.Count);
        Assert.Contains(tasksWithinWeek, t => t.Id == task1.Id);
        Assert.Contains(tasksWithinWeek, t => t.Id == task2.Id);
    }

    [Fact]
    public void GetTasksWithDueDates_ShouldReturnOnlyTasksWithDueDates()
    {
        // Arrange
        var service = new TodoService();
        var taskWithDate = service.AddTask("Task with Date", DateTime.Today.AddDays(1));
        var taskWithoutDate = service.AddTask("Task without Date");

        // Act
        var tasksWithDates = service.GetTasksWithDueDates();

        // Assert
        Assert.Single(tasksWithDates);
        Assert.Equal(taskWithDate.Id, tasksWithDates[0].Id);
    }

    [Fact]
    public void GetTasksWithoutDueDates_ShouldReturnOnlyTasksWithoutDueDates()
    {
        // Arrange
        var service = new TodoService();
        var taskWithDate = service.AddTask("Task with Date", DateTime.Today.AddDays(1));
        var taskWithoutDate1 = service.AddTask("Task without Date 1");
        var taskWithoutDate2 = service.AddTask("Task without Date 2");

        // Act
        var tasksWithoutDates = service.GetTasksWithoutDueDates();

        // Assert
        Assert.Equal(2, tasksWithoutDates.Count);
        Assert.Contains(tasksWithoutDates, t => t.Id == taskWithoutDate1.Id);
        Assert.Contains(tasksWithoutDates, t => t.Id == taskWithoutDate2.Id);
    }

    // Search and Filter Tests

    [Fact]
    public void SearchTasks_WithValidSearchTerm_ShouldReturnMatchingTasks()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Buy milk");
        service.AddTask("Buy bread");
        service.AddTask("Walk the dog");
        service.AddTask("Feed the cat");

        // Act
        var results = service.SearchTasks("Buy");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, task => Assert.Contains("Buy", task.Title));
    }

    [Fact]
    public void SearchTasks_CaseInsensitive_ShouldReturnMatchingTasks()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Buy Milk");
        service.AddTask("buy bread");
        service.AddTask("BUY EGGS");

        // Act
        var results = service.SearchTasks("buy");

        // Assert
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void SearchTasks_WithEmptySearchTerm_ShouldThrowArgumentException()
    {
        // Arrange
        var service = new TodoService();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.SearchTasks(""));
        Assert.Throws<ArgumentException>(() => service.SearchTasks(null!));
        Assert.Throws<ArgumentException>(() => service.SearchTasks("   "));
    }

    [Fact]
    public void SearchTasks_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Buy milk");
        service.AddTask("Walk dog");

        // Act
        var results = service.SearchTasks("xyz");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void SearchTasksWithFilters_WithCompletionFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var service = new TodoService();
        var task1 = service.AddTask("Buy milk");
        var task2 = service.AddTask("Buy bread");
        service.ToggleTaskCompletion(task1.Id);

        // Act
        var completedResults = service.SearchTasksWithFilters("Buy", isCompleted: true);
        var pendingResults = service.SearchTasksWithFilters("Buy", isCompleted: false);

        // Assert
        Assert.Single(completedResults);
        Assert.Equal(task1.Id, completedResults[0].Id);
        Assert.Single(pendingResults);
        Assert.Equal(task2.Id, pendingResults[0].Id);
    }

    [Fact]
    public void SearchTasksWithFilters_WithPriorityFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var service = new TodoService();
        var highTask = service.AddTask("Important task", todo.app.Models.TaskPriority.High);
        var mediumTask = service.AddTask("Important meeting", todo.app.Models.TaskPriority.Medium);
        service.AddTask("Random task", todo.app.Models.TaskPriority.Low);

        // Act
        var results = service.SearchTasksWithFilters("task", priority: todo.app.Models.TaskPriority.High);

        // Assert
        Assert.Single(results);
        Assert.Equal(highTask.Id, results[0].Id);
    }

    [Fact]
    public void SearchTasksWithFilters_WithOverdueFilter_ShouldReturnOverdueTasks()
    {
        // Arrange
        var service = new TodoService();
        var overdueTask = service.AddTask("Overdue task", DateTime.Today.AddDays(-1));
        var futureTask = service.AddTask("Future task", DateTime.Today.AddDays(1));

        // Act
        var results = service.SearchTasksWithFilters("task", includeOverdue: true);

        // Assert
        Assert.Single(results);
        Assert.Equal(overdueTask.Id, results[0].Id);
    }

    [Fact]
    public void SearchTasksWithFilters_WithDueTodayFilter_ShouldReturnTasksDueToday()
    {
        // Arrange
        var service = new TodoService();
        var todayTask = service.AddTask("Today task", DateTime.Today);
        var futureTask = service.AddTask("Future task", DateTime.Today.AddDays(1));

        // Act
        var results = service.SearchTasksWithFilters("task", includeDueToday: true);

        // Assert
        Assert.Single(results);
        Assert.Equal(todayTask.Id, results[0].Id);
    }

    [Fact]
    public void FilterTasks_WithMultipleCriteria_ShouldReturnMatchingTasks()
    {
        // Arrange
        var service = new TodoService();
        var task1 = service.AddTask("High priority task", todo.app.Models.TaskPriority.High);
        var task2 = service.AddTask("Medium priority task", todo.app.Models.TaskPriority.Medium);
        var task3 = service.AddTask("Low priority task", todo.app.Models.TaskPriority.Low);
        service.ToggleTaskCompletion(task2.Id);

        // Act
        var results = service.FilterTasks(isCompleted: false, priority: todo.app.Models.TaskPriority.High);

        // Assert
        Assert.Single(results);
        Assert.Equal(task1.Id, results[0].Id);
    }

    [Fact]
    public void FilterTasks_WithDateRange_ShouldReturnTasksInRange()
    {
        // Arrange
        var service = new TodoService();
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(2);
        
        var taskInRange = service.AddTask("Task in range", DateTime.Today.AddDays(1));
        var taskOutOfRange = service.AddTask("Task out of range", DateTime.Today.AddDays(5));

        // Act
        var results = service.FilterTasks(dueDateRange: (startDate, endDate));

        // Assert
        Assert.Single(results);
        Assert.Equal(taskInRange.Id, results[0].Id);
    }

    [Fact]
    public void SearchTasksMultipleTerms_WithOrOperation_ShouldReturnTasksMatchingAnyTerm()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Buy milk");
        service.AddTask("Walk dog");
        service.AddTask("Feed cat");
        service.AddTask("Clean house");

        // Act
        var results = service.SearchTasksMultipleTerms("milk", "dog");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, t => t.Title.Contains("milk"));
        Assert.Contains(results, t => t.Title.Contains("dog"));
    }

    [Fact]
    public void SearchTasksMultipleTerms_WithEmptyArray_ShouldThrowArgumentException()
    {
        // Arrange
        var service = new TodoService();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.SearchTasksMultipleTerms());
        Assert.Throws<ArgumentException>(() => service.SearchTasksMultipleTerms(null!));
        Assert.Throws<ArgumentException>(() => service.SearchTasksMultipleTerms("", "   "));
    }

    [Fact]
    public void SearchTasksAllTerms_WithAndOperation_ShouldReturnTasksMatchingAllTerms()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Buy fresh milk");
        service.AddTask("Buy bread");
        service.AddTask("Fresh fruit");

        // Act
        var results = service.SearchTasksAllTerms("Buy", "fresh");

        // Assert
        Assert.Single(results);
        Assert.Contains("Buy fresh milk", results[0].Title);
    }

    [Fact]
    public void SearchTasksAllTerms_WithNoMatchingTasks_ShouldReturnEmptyList()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Buy milk");
        service.AddTask("Walk dog");

        // Act
        var results = service.SearchTasksAllTerms("Buy", "dog");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void GetTasksSorted_ByTitle_ShouldReturnTasksInTitleOrder()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Zebra task");
        service.AddTask("Alpha task");
        service.AddTask("Beta task");

        // Act
        var results = service.GetTasksSorted(todo.app.Services.TaskSortCriterion.Title);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("Alpha task", results[0].Title);
        Assert.Equal("Beta task", results[1].Title);
        Assert.Equal("Zebra task", results[2].Title);
    }

    [Fact]
    public void GetTasksSorted_ByTitleDescending_ShouldReturnTasksInReverseTitleOrder()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Alpha task");
        service.AddTask("Beta task");
        service.AddTask("Zebra task");

        // Act
        var results = service.GetTasksSorted(todo.app.Services.TaskSortCriterion.Title, descendingPrimary: true);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("Zebra task", results[0].Title);
        Assert.Equal("Beta task", results[1].Title);
        Assert.Equal("Alpha task", results[2].Title);
    }

    [Fact]
    public void GetTasksSorted_ByPriority_ShouldReturnTasksInPriorityOrder()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("Low task", todo.app.Models.TaskPriority.Low);
        service.AddTask("High task", todo.app.Models.TaskPriority.High);
        service.AddTask("Medium task", todo.app.Models.TaskPriority.Medium);

        // Act
        var results = service.GetTasksSorted(todo.app.Services.TaskSortCriterion.Priority);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(todo.app.Models.TaskPriority.Low, results[0].Priority);
        Assert.Equal(todo.app.Models.TaskPriority.Medium, results[1].Priority);
        Assert.Equal(todo.app.Models.TaskPriority.High, results[2].Priority);
    }

    [Fact]
    public void GetTasksSorted_ByDueDate_ShouldReturnTasksInDueDateOrder()
    {
        // Arrange
        var service = new TodoService();
        var futureDate = DateTime.Today.AddDays(3);
        var todayDate = DateTime.Today;
        var middleDate = DateTime.Today.AddDays(1);

        service.AddTask("Future task", futureDate);
        service.AddTask("Today task", todayDate);
        service.AddTask("Middle task", middleDate);
        service.AddTask("No date task"); // Should come last

        // Act
        var results = service.GetTasksSorted(todo.app.Services.TaskSortCriterion.DueDate);

        // Assert
        Assert.Equal(4, results.Count);
        Assert.Equal(todayDate, results[0].DueDate);
        Assert.Equal(middleDate, results[1].DueDate);
        Assert.Equal(futureDate, results[2].DueDate);
        Assert.Null(results[3].DueDate); // Tasks without due dates come last
    }

    [Fact]
    public void GetTasksSorted_ByCompletionStatus_ShouldReturnIncompleteTasksFirst()
    {
        // Arrange
        var service = new TodoService();
        var task1 = service.AddTask("Task 1");
        var task2 = service.AddTask("Task 2");
        var task3 = service.AddTask("Task 3");
        
        service.ToggleTaskCompletion(task2.Id);

        // Act
        var results = service.GetTasksSorted(todo.app.Services.TaskSortCriterion.CompletionStatus);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.False(results[0].IsCompleted);
        Assert.False(results[1].IsCompleted);
        Assert.True(results[2].IsCompleted);
    }

    [Fact]
    public void GetTasksSorted_WithSecondarySort_ShouldApplyBothCriteria()
    {
        // Arrange
        var service = new TodoService();
        service.AddTask("B Task", todo.app.Models.TaskPriority.High);
        service.AddTask("A Task", todo.app.Models.TaskPriority.High);
        service.AddTask("C Task", todo.app.Models.TaskPriority.Low);

        // Act - Sort by priority first, then by title
        var results = service.GetTasksSorted(
            todo.app.Services.TaskSortCriterion.Priority,
            todo.app.Services.TaskSortCriterion.Title);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("C Task", results[0].Title); // Low priority first
        Assert.Equal("A Task", results[1].Title); // High priority, A comes before B
        Assert.Equal("B Task", results[2].Title); // High priority, B comes after A
    }
}
