using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using todo.app.Models;

namespace todo.app.Services;

/// <summary>
/// Service responsible for managing todo tasks
/// </summary>
public class TodoService
{
    private readonly List<TodoTask> _tasks;
    private readonly DataPersistenceService _persistenceService;

    /// <summary>
    /// Gets all tasks in the service
    /// </summary>
    public IReadOnlyList<TodoTask> Tasks => _tasks.AsReadOnly();

    /// <summary>
    /// Gets the number of total tasks
    /// </summary>
    public int TotalTaskCount => _tasks.Count;

    /// <summary>
    /// Gets the number of completed tasks
    /// </summary>
    public int CompletedTaskCount => _tasks.Count(t => t.IsCompleted);

    /// <summary>
    /// Gets the number of pending (incomplete) tasks
    /// </summary>
    public int PendingTaskCount => _tasks.Count(t => !t.IsCompleted);

    /// <summary>
    /// Creates a new TodoService instance
    /// </summary>
    /// <param name="persistenceService">The persistence service to use. If null, creates a default one.</param>
    public TodoService(DataPersistenceService? persistenceService = null)
    {
        _tasks = new List<TodoTask>();
        _persistenceService = persistenceService ?? new DataPersistenceService();
    }

    /// <summary>
    /// Adds a new task with the specified title
    /// </summary>
    /// <param name="title">The task title</param>
    /// <returns>The created task</returns>
    /// <exception cref="ArgumentException">Thrown when title is null, empty, or whitespace</exception>
    public TodoTask AddTask(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title cannot be null, empty, or whitespace", nameof(title));
        }

        var task = new TodoTask(title.Trim());
        _tasks.Add(task);
        return task;
    }

    /// <summary>
    /// Removes a task from the service
    /// </summary>
    /// <param name="taskId">The ID of the task to remove</param>
    /// <returns>True if the task was found and removed, false otherwise</returns>
    public bool RemoveTask(Guid taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            _tasks.Remove(task);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets a task by its ID
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <returns>The task if found, null otherwise</returns>
    public TodoTask? GetTask(Guid taskId)
    {
        return _tasks.FirstOrDefault(t => t.Id == taskId);
    }

    /// <summary>
    /// Toggles the completion status of a task
    /// </summary>
    /// <param name="taskId">The ID of the task to toggle</param>
    /// <returns>True if the task was found and toggled, false otherwise</returns>
    public bool ToggleTaskCompletion(Guid taskId)
    {
        var task = GetTask(taskId);
        if (task != null)
        {
            if (task.IsCompleted)
            {
                task.MarkAsIncomplete();
            }
            else
            {
                task.MarkAsCompleted();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates the title of an existing task
    /// </summary>
    /// <param name="taskId">The ID of the task to edit</param>
    /// <param name="newTitle">The new title for the task</param>
    /// <returns>True if the task was found and updated, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when newTitle is null, empty, or whitespace</exception>
    public bool EditTask(Guid taskId, string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException("Task title cannot be null, empty, or whitespace", nameof(newTitle));
        }

        var task = GetTask(taskId);
        if (task != null)
        {
            task.UpdateTitle(newTitle.Trim());
            return true;
        }
        return false;
    }

    /// <summary>
    /// Clears all tasks from the service
    /// </summary>
    public void ClearAllTasks()
    {
        _tasks.Clear();
    }

    /// <summary>
    /// Gets all completed tasks
    /// </summary>
    /// <returns>A list of completed tasks</returns>
    public IReadOnlyList<TodoTask> GetCompletedTasks()
    {
        return _tasks.Where(t => t.IsCompleted).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all pending (incomplete) tasks
    /// </summary>
    /// <returns>A list of pending tasks</returns>
    public IReadOnlyList<TodoTask> GetPendingTasks()
    {
        return _tasks.Where(t => !t.IsCompleted).ToList().AsReadOnly();
    }

    /// <summary>
    /// Loads tasks from persistent storage
    /// </summary>
    /// <returns>A task representing the async operation</returns>
    public async Task LoadTasksAsync()
    {
        var loadedTasks = await _persistenceService.LoadTasksAsync();
        _tasks.Clear();
        _tasks.AddRange(loadedTasks);
    }

    /// <summary>
    /// Saves tasks to persistent storage
    /// </summary>
    /// <returns>A task representing the async operation</returns>
    public async Task SaveTasksAsync()
    {
        await _persistenceService.SaveTasksAsync(_tasks);
    }

    /// <summary>
    /// Adds a new task and automatically saves to storage
    /// </summary>
    /// <param name="title">The task title</param>
    /// <returns>The created task</returns>
    /// <exception cref="ArgumentException">Thrown when title is null, empty, or whitespace</exception>
    public async Task<TodoTask> AddTaskAsync(string title)
    {
        var task = AddTask(title);
        await SaveTasksAsync();
        return task;
    }

    /// <summary>
    /// Removes a task and automatically saves to storage
    /// </summary>
    /// <param name="taskId">The ID of the task to remove</param>
    /// <returns>True if the task was found and removed, false otherwise</returns>
    public async Task<bool> RemoveTaskAsync(Guid taskId)
    {
        var result = RemoveTask(taskId);
        if (result)
        {
            await SaveTasksAsync();
        }
        return result;
    }

    /// <summary>
    /// Toggles task completion and automatically saves to storage
    /// </summary>
    /// <param name="taskId">The ID of the task to toggle</param>
    /// <returns>True if the task was found and toggled, false otherwise</returns>
    public async Task<bool> ToggleTaskCompletionAsync(Guid taskId)
    {
        var result = ToggleTaskCompletion(taskId);
        if (result)
        {
            await SaveTasksAsync();
        }
        return result;
    }

    /// <summary>
    /// Edits a task and automatically saves to storage
    /// </summary>
    /// <param name="taskId">The ID of the task to edit</param>
    /// <param name="newTitle">The new title for the task</param>
    /// <returns>True if the task was found and updated, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when newTitle is null, empty, or whitespace</exception>
    public async Task<bool> EditTaskAsync(Guid taskId, string newTitle)
    {
        var result = EditTask(taskId, newTitle);
        if (result)
        {
            await SaveTasksAsync();
        }
        return result;
    }
}
