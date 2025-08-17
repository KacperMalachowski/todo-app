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
    /// Creates a new TodoTask with default values
    /// </summary>
    public TodoTask()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsCompleted = false;
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
}
