using System;

namespace todo.app.Models;

/// <summary>
/// Represents the priority level of a todo task
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Low priority task
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium priority task (default)
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High priority task
    /// </summary>
    High = 2
}
