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
}
