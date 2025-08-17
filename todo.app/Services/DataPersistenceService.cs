using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using todo.app.Models;

namespace todo.app.Services;

/// <summary>
/// Service responsible for persisting todo tasks to and from storage
/// </summary>
public class DataPersistenceService
{
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new DataPersistenceService instance
    /// </summary>
    /// <param name="dataFilePath">Path to the data file. If null, uses default location.</param>
    public DataPersistenceService(string? dataFilePath = null)
    {
        _dataFilePath = dataFilePath ?? GetDefaultDataFilePath();
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(_dataFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Gets the default data file path
    /// </summary>
    /// <returns>Default path for the todo data file</returns>
    private static string GetDefaultDataFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "TodoApp");
        return Path.Combine(appFolder, "tasks.json");
    }

    /// <summary>
    /// Saves tasks to the data file
    /// </summary>
    /// <param name="tasks">The tasks to save</param>
    /// <returns>A task representing the async operation</returns>
    public async Task SaveTasksAsync(IEnumerable<TodoTask> tasks)
    {
        try
        {
            var json = JsonSerializer.Serialize(tasks, _jsonOptions);
            await File.WriteAllTextAsync(_dataFilePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save tasks to {_dataFilePath}", ex);
        }
    }

    /// <summary>
    /// Loads tasks from the data file
    /// </summary>
    /// <returns>The loaded tasks, or an empty list if file doesn't exist</returns>
    public async Task<List<TodoTask>> LoadTasksAsync()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                return new List<TodoTask>();
            }

            var json = await File.ReadAllTextAsync(_dataFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<TodoTask>();
            }

            var tasks = JsonSerializer.Deserialize<List<TodoTask>>(json, _jsonOptions);
            return tasks ?? new List<TodoTask>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load tasks from {_dataFilePath}", ex);
        }
    }

    /// <summary>
    /// Checks if the data file exists
    /// </summary>
    /// <returns>True if the data file exists, false otherwise</returns>
    public bool DataFileExists()
    {
        return File.Exists(_dataFilePath);
    }

    /// <summary>
    /// Deletes the data file if it exists
    /// </summary>
    /// <returns>True if the file was deleted, false if it didn't exist</returns>
    public bool DeleteDataFile()
    {
        if (File.Exists(_dataFilePath))
        {
            File.Delete(_dataFilePath);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the path to the data file
    /// </summary>
    /// <returns>The data file path</returns>
    public string GetDataFilePath()
    {
        return _dataFilePath;
    }
}
