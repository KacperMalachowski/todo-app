using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using todo.app.Models;
using todo.app.Services;
using Xunit;

namespace todo.Tests;

public class DataPersistenceServiceTests
{
    private string GetTestFilePath()
    {
        return Path.Combine(Path.GetTempPath(), $"test_tasks_{Guid.NewGuid()}.json");
    }

    [Fact]
    public async Task SaveTasksAsync_WithValidTasks_ShouldCreateFile()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);
        var tasks = new List<TodoTask>
        {
            new TodoTask("Task 1"),
            new TodoTask("Task 2")
        };

        try
        {
            // Act
            await service.SaveTasksAsync(tasks);

            // Assert
            Assert.True(File.Exists(testFile));
            var fileContent = await File.ReadAllTextAsync(testFile);
            Assert.False(string.IsNullOrWhiteSpace(fileContent));
            Assert.Contains("Task 1", fileContent);
            Assert.Contains("Task 2", fileContent);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task LoadTasksAsync_WithExistingFile_ShouldReturnTasks()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);
        var originalTasks = new List<TodoTask>
        {
            new TodoTask("Task 1"),
            new TodoTask("Task 2") { IsCompleted = true, CompletedAt = DateTime.UtcNow }
        };

        try
        {
            // Save tasks first
            await service.SaveTasksAsync(originalTasks);

            // Act
            var loadedTasks = await service.LoadTasksAsync();

            // Assert
            Assert.Equal(2, loadedTasks.Count);
            Assert.Contains(loadedTasks, t => t.Title == "Task 1" && !t.IsCompleted);
            Assert.Contains(loadedTasks, t => t.Title == "Task 2" && t.IsCompleted);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task LoadTasksAsync_WithNonExistentFile_ShouldReturnEmptyList()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);

        // Act
        var loadedTasks = await service.LoadTasksAsync();

        // Assert
        Assert.Empty(loadedTasks);
    }

    [Fact]
    public async Task LoadTasksAsync_WithEmptyFile_ShouldReturnEmptyList()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);

        try
        {
            // Create empty file
            await File.WriteAllTextAsync(testFile, string.Empty);

            // Act
            var loadedTasks = await service.LoadTasksAsync();

            // Assert
            Assert.Empty(loadedTasks);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public async Task SaveLoadRoundTrip_ShouldPreserveAllTaskProperties()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);
        var originalTask = new TodoTask("Test Task");
        originalTask.MarkAsCompleted();
        var originalTasks = new List<TodoTask> { originalTask };

        try
        {
            // Act
            await service.SaveTasksAsync(originalTasks);
            var loadedTasks = await service.LoadTasksAsync();

            // Assert
            Assert.Single(loadedTasks);
            var loadedTask = loadedTasks[0];
            Assert.Equal(originalTask.Id, loadedTask.Id);
            Assert.Equal(originalTask.Title, loadedTask.Title);
            Assert.Equal(originalTask.IsCompleted, loadedTask.IsCompleted);
            Assert.Equal(originalTask.CreatedAt, loadedTask.CreatedAt);
            Assert.Equal(originalTask.CompletedAt, loadedTask.CompletedAt);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public void DataFileExists_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);

        try
        {
            // Create file
            File.WriteAllText(testFile, "test");

            // Act & Assert
            Assert.True(service.DataFileExists());
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }
        }
    }

    [Fact]
    public void DataFileExists_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);

        // Act & Assert
        Assert.False(service.DataFileExists());
    }

    [Fact]
    public void DeleteDataFile_WithExistingFile_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);
        File.WriteAllText(testFile, "test");

        // Act
        var result = service.DeleteDataFile();

        // Assert
        Assert.True(result);
        Assert.False(File.Exists(testFile));
    }

    [Fact]
    public void DeleteDataFile_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);

        // Act
        var result = service.DeleteDataFile();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetDataFilePath_ShouldReturnCorrectPath()
    {
        // Arrange
        var testFile = GetTestFilePath();
        var service = new DataPersistenceService(testFile);

        // Act
        var returnedPath = service.GetDataFilePath();

        // Assert
        Assert.Equal(testFile, returnedPath);
    }

    [Fact]
    public async Task SaveTasksAsync_WithReadOnlyPath_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var testFile = Path.Combine(Path.GetTempPath(), "readonly_test.json");
        var service = new DataPersistenceService(testFile);
        var tasks = new List<TodoTask> { new TodoTask("Test") };

        try
        {
            // Create the file and make it read-only to simulate write failure
            await File.WriteAllTextAsync(testFile, "existing content");
            File.SetAttributes(testFile, FileAttributes.ReadOnly);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveTasksAsync(tasks));
        }
        finally
        {
            // Cleanup - remove read-only attribute first
            if (File.Exists(testFile))
            {
                File.SetAttributes(testFile, FileAttributes.Normal);
                File.Delete(testFile);
            }
        }
    }
}
