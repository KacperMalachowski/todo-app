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

    /// <summary>
    /// Adds a new task with the specified title and priority
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="priority">The task priority</param>
    /// <returns>The created task</returns>
    /// <exception cref="ArgumentException">Thrown when title is null, empty, or whitespace</exception>
    public TodoTask AddTask(string title, TaskPriority priority)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title cannot be null, empty, or whitespace", nameof(title));
        }

        var task = new TodoTask(title.Trim(), priority);
        _tasks.Add(task);
        return task;
    }

    /// <summary>
    /// Adds a new task with priority and automatically saves to storage
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="priority">The task priority</param>
    /// <returns>The created task</returns>
    /// <exception cref="ArgumentException">Thrown when title is null, empty, or whitespace</exception>
    public async Task<TodoTask> AddTaskAsync(string title, TaskPriority priority)
    {
        var task = AddTask(title, priority);
        await SaveTasksAsync();
        return task;
    }

    /// <summary>
    /// Updates the priority of an existing task
    /// </summary>
    /// <param name="taskId">The ID of the task to update</param>
    /// <param name="newPriority">The new priority for the task</param>
    /// <returns>True if the task was found and updated, false otherwise</returns>
    public bool UpdateTaskPriority(Guid taskId, TaskPriority newPriority)
    {
        var task = GetTask(taskId);
        if (task != null)
        {
            task.UpdatePriority(newPriority);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates task priority and automatically saves to storage
    /// </summary>
    /// <param name="taskId">The ID of the task to update</param>
    /// <param name="newPriority">The new priority for the task</param>
    /// <returns>True if the task was found and updated, false otherwise</returns>
    public async Task<bool> UpdateTaskPriorityAsync(Guid taskId, TaskPriority newPriority)
    {
        var result = UpdateTaskPriority(taskId, newPriority);
        if (result)
        {
            await SaveTasksAsync();
        }
        return result;
    }

    /// <summary>
    /// Gets all tasks with the specified priority
    /// </summary>
    /// <param name="priority">The priority to filter by</param>
    /// <returns>A list of tasks with the specified priority</returns>
    public IReadOnlyList<TodoTask> GetTasksByPriority(TaskPriority priority)
    {
        return _tasks.Where(t => t.Priority == priority).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all high priority tasks
    /// </summary>
    /// <returns>A list of high priority tasks</returns>
    public IReadOnlyList<TodoTask> GetHighPriorityTasks()
    {
        return GetTasksByPriority(TaskPriority.High);
    }

    /// <summary>
    /// Gets all medium priority tasks
    /// </summary>
    /// <returns>A list of medium priority tasks</returns>
    public IReadOnlyList<TodoTask> GetMediumPriorityTasks()
    {
        return GetTasksByPriority(TaskPriority.Medium);
    }

    /// <summary>
    /// Gets all low priority tasks
    /// </summary>
    /// <returns>A list of low priority tasks</returns>
    public IReadOnlyList<TodoTask> GetLowPriorityTasks()
    {
        return GetTasksByPriority(TaskPriority.Low);
    }

    // Due Date Methods

    /// <summary>
    /// Adds a new task with the specified title and due date
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="dueDate">The task due date</param>
    /// <returns>The created task</returns>
    /// <exception cref="ArgumentException">Thrown when title is null, empty, or whitespace</exception>
    public TodoTask AddTask(string title, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title cannot be null, empty, or whitespace", nameof(title));
        }

        var task = new TodoTask(title.Trim(), dueDate);
        _tasks.Add(task);
        return task;
    }

    /// <summary>
    /// Adds a new task with title, priority, and due date
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="priority">The task priority</param>
    /// <param name="dueDate">The task due date</param>
    /// <returns>The created task</returns>
    /// <exception cref="ArgumentException">Thrown when title is null, empty, or whitespace</exception>
    public TodoTask AddTask(string title, TaskPriority priority, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title cannot be null, empty, or whitespace", nameof(title));
        }

        var task = new TodoTask(title.Trim(), priority, dueDate);
        _tasks.Add(task);
        return task;
    }

    /// <summary>
    /// Adds a new task with due date and automatically saves to storage
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="dueDate">The task due date</param>
    /// <returns>The created task</returns>
    /// <exception cref="ArgumentException">Thrown when title is null, empty, or whitespace</exception>
    public async Task<TodoTask> AddTaskAsync(string title, DateTime dueDate)
    {
        var task = AddTask(title, dueDate);
        await SaveTasksAsync();
        return task;
    }

    /// <summary>
    /// Adds a new task with priority and due date and automatically saves to storage
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="priority">The task priority</param>
    /// <param name="dueDate">The task due date</param>
    /// <returns>The created task</returns>
    /// <exception cref="ArgumentException">Thrown when title is null, empty, or whitespace</exception>
    public async Task<TodoTask> AddTaskAsync(string title, TaskPriority priority, DateTime dueDate)
    {
        var task = AddTask(title, priority, dueDate);
        await SaveTasksAsync();
        return task;
    }

    /// <summary>
    /// Updates the due date of an existing task
    /// </summary>
    /// <param name="taskId">The ID of the task to update</param>
    /// <param name="newDueDate">The new due date for the task (null to remove due date)</param>
    /// <returns>True if the task was found and updated, false otherwise</returns>
    public bool UpdateTaskDueDate(Guid taskId, DateTime? newDueDate)
    {
        var task = GetTask(taskId);
        if (task != null)
        {
            task.UpdateDueDate(newDueDate);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates task due date and automatically saves to storage
    /// </summary>
    /// <param name="taskId">The ID of the task to update</param>
    /// <param name="newDueDate">The new due date for the task (null to remove due date)</param>
    /// <returns>True if the task was found and updated, false otherwise</returns>
    public async Task<bool> UpdateTaskDueDateAsync(Guid taskId, DateTime? newDueDate)
    {
        var result = UpdateTaskDueDate(taskId, newDueDate);
        if (result)
        {
            await SaveTasksAsync();
        }
        return result;
    }

    /// <summary>
    /// Gets all tasks that are overdue
    /// </summary>
    /// <returns>A list of overdue tasks</returns>
    public IReadOnlyList<TodoTask> GetOverdueTasks()
    {
        return _tasks.Where(t => t.IsOverdue).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all tasks due today
    /// </summary>
    /// <returns>A list of tasks due today</returns>
    public IReadOnlyList<TodoTask> GetTasksDueToday()
    {
        return _tasks.Where(t => t.IsDueToday).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all tasks due within the specified number of days
    /// </summary>
    /// <param name="days">Number of days to check</param>
    /// <returns>A list of tasks due within the specified days</returns>
    public IReadOnlyList<TodoTask> GetTasksDueWithin(int days)
    {
        return _tasks.Where(t => t.IsDueWithin(days)).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all tasks with due dates
    /// </summary>
    /// <returns>A list of tasks that have due dates</returns>
    public IReadOnlyList<TodoTask> GetTasksWithDueDates()
    {
        return _tasks.Where(t => t.DueDate.HasValue).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets all tasks without due dates
    /// </summary>
    /// <returns>A list of tasks that don't have due dates</returns>
    public IReadOnlyList<TodoTask> GetTasksWithoutDueDates()
    {
        return _tasks.Where(t => !t.DueDate.HasValue).ToList().AsReadOnly();
    }

    // Search and Filter Methods

    /// <summary>
    /// Searches tasks by title containing the specified search term (case-insensitive)
    /// </summary>
    /// <param name="searchTerm">The search term to look for in task titles</param>
    /// <returns>A list of tasks containing the search term in their title</returns>
    /// <exception cref="ArgumentException">Thrown when searchTerm is null or empty</exception>
    public IReadOnlyList<TodoTask> SearchTasks(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));
        }

        return _tasks
            .Where(task => task.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Searches tasks by title containing the search term with additional filters
    /// </summary>
    /// <param name="searchTerm">The search term to look for in task titles</param>
    /// <param name="isCompleted">Optional filter for completion status</param>
    /// <param name="priority">Optional filter for priority level</param>
    /// <param name="includeOverdue">Optional filter to include only overdue tasks</param>
    /// <param name="includeDueToday">Optional filter to include only tasks due today</param>
    /// <returns>A list of tasks matching the search and filter criteria</returns>
    /// <exception cref="ArgumentException">Thrown when searchTerm is null or empty</exception>
    public IReadOnlyList<TodoTask> SearchTasksWithFilters(
        string searchTerm,
        bool? isCompleted = null,
        TaskPriority? priority = null,
        bool includeOverdue = false,
        bool includeDueToday = false)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));
        }

        var query = _tasks
            .Where(task => task.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

        // Apply completion status filter
        if (isCompleted.HasValue)
        {
            query = query.Where(task => task.IsCompleted == isCompleted.Value);
        }

        // Apply priority filter
        if (priority.HasValue)
        {
            query = query.Where(task => task.Priority == priority.Value);
        }

        // Apply due date filters
        if (includeOverdue)
        {
            query = query.Where(task => task.IsOverdue);
        }
        else if (includeDueToday)
        {
            query = query.Where(task => task.IsDueToday);
        }

        return query.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets tasks matching multiple filter criteria without requiring a search term
    /// </summary>
    /// <param name="isCompleted">Optional filter for completion status</param>
    /// <param name="priority">Optional filter for priority level</param>
    /// <param name="includeOverdue">Optional filter to include only overdue tasks</param>
    /// <param name="includeDueToday">Optional filter to include only tasks due today</param>
    /// <param name="dueDateRange">Optional filter for tasks due within a specific date range</param>
    /// <returns>A list of tasks matching the filter criteria</returns>
    public IReadOnlyList<TodoTask> FilterTasks(
        bool? isCompleted = null,
        TaskPriority? priority = null,
        bool includeOverdue = false,
        bool includeDueToday = false,
        (DateTime startDate, DateTime endDate)? dueDateRange = null)
    {
        var query = _tasks.AsQueryable();

        // Apply completion status filter
        if (isCompleted.HasValue)
        {
            query = query.Where(task => task.IsCompleted == isCompleted.Value);
        }

        // Apply priority filter
        if (priority.HasValue)
        {
            query = query.Where(task => task.Priority == priority.Value);
        }

        // Apply due date filters
        if (includeOverdue)
        {
            query = query.Where(task => task.IsOverdue);
        }
        else if (includeDueToday)
        {
            query = query.Where(task => task.IsDueToday);
        }
        else if (dueDateRange.HasValue)
        {
            var (startDate, endDate) = dueDateRange.Value;
            query = query.Where(task => task.DueDate.HasValue &&
                                       task.DueDate.Value.Date >= startDate.Date &&
                                       task.DueDate.Value.Date <= endDate.Date);
        }

        return query.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets tasks that match any of the provided search terms (OR operation)
    /// </summary>
    /// <param name="searchTerms">Array of search terms</param>
    /// <returns>A list of tasks containing any of the search terms</returns>
    /// <exception cref="ArgumentException">Thrown when searchTerms is null or empty</exception>
    public IReadOnlyList<TodoTask> SearchTasksMultipleTerms(params string[] searchTerms)
    {
        if (searchTerms == null || searchTerms.Length == 0)
        {
            throw new ArgumentException("At least one search term must be provided", nameof(searchTerms));
        }

        var validTerms = searchTerms.Where(term => !string.IsNullOrWhiteSpace(term)).ToArray();
        if (validTerms.Length == 0)
        {
            throw new ArgumentException("At least one non-empty search term must be provided", nameof(searchTerms));
        }

        return _tasks
            .Where(task => validTerms.Any(term =>
                task.Title.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets tasks that contain all provided search terms (AND operation)
    /// </summary>
    /// <param name="searchTerms">Array of search terms that must all be present</param>
    /// <returns>A list of tasks containing all search terms</returns>
    /// <exception cref="ArgumentException">Thrown when searchTerms is null or empty</exception>
    public IReadOnlyList<TodoTask> SearchTasksAllTerms(params string[] searchTerms)
    {
        if (searchTerms == null || searchTerms.Length == 0)
        {
            throw new ArgumentException("At least one search term must be provided", nameof(searchTerms));
        }

        var validTerms = searchTerms.Where(term => !string.IsNullOrWhiteSpace(term)).ToArray();
        if (validTerms.Length == 0)
        {
            throw new ArgumentException("At least one non-empty search term must be provided", nameof(searchTerms));
        }

        return _tasks
            .Where(task => validTerms.All(term =>
                task.Title.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets tasks sorted by multiple criteria
    /// </summary>
    /// <param name="primarySort">Primary sort criterion</param>
    /// <param name="secondarySort">Secondary sort criterion</param>
    /// <param name="descendingPrimary">Whether to sort primary criterion in descending order</param>
    /// <param name="descendingSecondary">Whether to sort secondary criterion in descending order</param>
    /// <returns>A list of tasks sorted by the specified criteria</returns>
    public IReadOnlyList<TodoTask> GetTasksSorted(
        TaskSortCriterion primarySort,
        TaskSortCriterion? secondarySort = null,
        bool descendingPrimary = false,
        bool descendingSecondary = false)
    {
        var query = _tasks.AsQueryable();

        // Apply primary sort
        query = ApplySortCriterion(query, primarySort, descendingPrimary);

        // Apply secondary sort if specified
        if (secondarySort.HasValue)
        {
            query = ApplySortCriterion(query, secondarySort.Value, descendingSecondary, isSecondary: true);
        }

        return query.ToList().AsReadOnly();
    }

    /// <summary>
    /// Helper method to apply sort criterion to a query
    /// </summary>
    private IQueryable<TodoTask> ApplySortCriterion(
        IQueryable<TodoTask> query,
        TaskSortCriterion sortCriterion,
        bool descending,
        bool isSecondary = false)
    {
        return sortCriterion switch
        {
            TaskSortCriterion.Title => isSecondary
                ? (descending ? ((IOrderedQueryable<TodoTask>)query).ThenByDescending(t => t.Title)
                              : ((IOrderedQueryable<TodoTask>)query).ThenBy(t => t.Title))
                : (descending ? query.OrderByDescending(t => t.Title)
                              : query.OrderBy(t => t.Title)),

            TaskSortCriterion.Priority => isSecondary
                ? (descending ? ((IOrderedQueryable<TodoTask>)query).ThenByDescending(t => t.Priority)
                              : ((IOrderedQueryable<TodoTask>)query).ThenBy(t => t.Priority))
                : (descending ? query.OrderByDescending(t => t.Priority)
                              : query.OrderBy(t => t.Priority)),

            TaskSortCriterion.DueDate => isSecondary
                ? (descending ? ((IOrderedQueryable<TodoTask>)query).ThenByDescending(t => t.DueDate ?? DateTime.MaxValue)
                              : ((IOrderedQueryable<TodoTask>)query).ThenBy(t => t.DueDate ?? DateTime.MaxValue))
                : (descending ? query.OrderByDescending(t => t.DueDate ?? DateTime.MaxValue)
                              : query.OrderBy(t => t.DueDate ?? DateTime.MaxValue)),

            TaskSortCriterion.CompletionStatus => isSecondary
                ? (descending ? ((IOrderedQueryable<TodoTask>)query).ThenByDescending(t => t.IsCompleted)
                              : ((IOrderedQueryable<TodoTask>)query).ThenBy(t => t.IsCompleted))
                : (descending ? query.OrderByDescending(t => t.IsCompleted)
                              : query.OrderBy(t => t.IsCompleted)),

            _ => query
        };
    }
}

/// <summary>
/// Enumeration of task sort criteria
/// </summary>
public enum TaskSortCriterion
{
    Title,
    Priority,
    CreatedAt,
    DueDate,
    CompletionStatus
}
