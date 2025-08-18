using System;

namespace todo.app.Models;

/// <summary>
/// Represents a single todo task item
/// </summary>
public class TodoTask
{
    /// <summary>
    /// Unique identifier for the task
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The text description of the task
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Whether the task has been completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// When the task was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the task was completed (if applicable)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// The priority level of the task
    /// </summary>
    public TaskPriority Priority { get; set; }

    /// <summary>
    /// When the task is due (optional)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Creates a new TodoTask with default values
    /// </summary>
    public TodoTask()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsCompleted = false;
        Priority = TaskPriority.Medium;
    }

    /// <summary>
    /// Creates a new TodoTask with the specified title
    /// </summary>
    /// <param name="title">The task title</param>
    public TodoTask(string title) : this()
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    /// <summary>
    /// Creates a new TodoTask with the specified title and priority
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="priority">The task priority</param>
    public TodoTask(string title, TaskPriority priority) : this(title)
    {
        Priority = priority;
    }

    /// <summary>
    /// Creates a new TodoTask with the specified title and due date
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="dueDate">The task due date</param>
    public TodoTask(string title, DateTime dueDate) : this(title)
    {
        DueDate = dueDate;
    }

    /// <summary>
    /// Creates a new TodoTask with the specified title, priority, and due date
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="priority">The task priority</param>
    /// <param name="dueDate">The task due date</param>
    public TodoTask(string title, TaskPriority priority, DateTime dueDate) : this(title, priority)
    {
        DueDate = dueDate;
    }

    /// <summary>
    /// Marks the task as completed
    /// </summary>
    public void MarkAsCompleted()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Marks the task as not completed
    /// </summary>
    public void MarkAsIncomplete()
    {
        IsCompleted = false;
        CompletedAt = null;
    }

    /// <summary>
    /// Updates the task title
    /// </summary>
    /// <param name="newTitle">The new title for the task</param>
    /// <exception cref="ArgumentException">Thrown when newTitle is null, empty, or whitespace</exception>
    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException("Task title cannot be null, empty, or whitespace", nameof(newTitle));
        }
        Title = newTitle;
    }

    /// <summary>
    /// Updates the task priority
    /// </summary>
    /// <param name="newPriority">The new priority for the task</param>
    public void UpdatePriority(TaskPriority newPriority)
    {
        Priority = newPriority;
    }

    /// <summary>
    /// Updates the task due date
    /// </summary>
    /// <param name="newDueDate">The new due date for the task (null to remove due date)</param>
    public void UpdateDueDate(DateTime? newDueDate)
    {
        DueDate = newDueDate;
    }

    /// <summary>
    /// Gets whether the task is overdue
    /// </summary>
    public bool IsOverdue
    {
        get
        {
            return DueDate.HasValue &&
                   !IsCompleted &&
                   DueDate.Value.Date < DateTime.Today;
        }
    }

    /// <summary>
    /// Gets whether the task is due today
    /// </summary>
    public bool IsDueToday
    {
        get
        {
            return DueDate.HasValue &&
                   !IsCompleted &&
                   DueDate.Value.Date == DateTime.Today;
        }
    }

    /// <summary>
    /// Gets whether the task is due within the specified number of days
    /// </summary>
    /// <param name="days">Number of days to check</param>
    /// <returns>True if the task is due within the specified days</returns>
    public bool IsDueWithin(int days)
    {
        if (!DueDate.HasValue || IsCompleted) return false;

        var targetDate = DateTime.Today.AddDays(days);
        return DueDate.Value.Date >= DateTime.Today && DueDate.Value.Date <= targetDate;
    }

    /// <summary>
    /// Gets the number of days until the task is due (negative if overdue)
    /// </summary>
    public int? DaysUntilDue
    {
        get
        {
            if (!DueDate.HasValue) return null;

            var days = (DueDate.Value.Date - DateTime.Today).Days;
            return days;
        }
    }
}
