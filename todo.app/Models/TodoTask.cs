using System;
using System.Collections.Generic;
using System.Linq;

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
    /// Categories/tags associated with this task
    /// </summary>
    public List<string> Categories { get; set; } = new();

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
    /// Creates a new TodoTask with the specified title and categories
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="categories">The task categories</param>
    public TodoTask(string title, IEnumerable<string> categories) : this(title)
    {
        Categories = categories?.Where(c => !string.IsNullOrWhiteSpace(c))
                               .Select(c => c.Trim())
                               .Distinct()
                               .ToList() ?? new List<string>();
    }

    /// <summary>
    /// Creates a new TodoTask with the specified title, priority, and categories
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="priority">The task priority</param>
    /// <param name="categories">The task categories</param>
    public TodoTask(string title, TaskPriority priority, IEnumerable<string> categories) : this(title, categories)
    {
        Priority = priority;
    }

    /// <summary>
    /// Creates a new TodoTask with the specified title, due date, and categories
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="dueDate">The task due date</param>
    /// <param name="categories">The task categories</param>
    public TodoTask(string title, DateTime dueDate, IEnumerable<string> categories) : this(title, categories)
    {
        DueDate = dueDate;
    }

    /// <summary>
    /// Creates a new TodoTask with the specified title, priority, due date, and categories
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="priority">The task priority</param>
    /// <param name="dueDate">The task due date</param>
    /// <param name="categories">The task categories</param>
    public TodoTask(string title, TaskPriority priority, DateTime dueDate, IEnumerable<string> categories) : this(title, priority, categories)
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

    // Category Management Methods

    /// <summary>
    /// Adds a category to the task if it doesn't already exist
    /// </summary>
    /// <param name="category">The category to add</param>
    /// <returns>True if the category was added, false if it already existed</returns>
    /// <exception cref="ArgumentException">Thrown when category is null, empty, or whitespace</exception>
    public bool AddCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category cannot be null, empty, or whitespace", nameof(category));
        }

        var trimmedCategory = category.Trim();
        if (Categories.Contains(trimmedCategory, StringComparer.OrdinalIgnoreCase))
        {
            return false; // Category already exists
        }

        Categories.Add(trimmedCategory);
        return true;
    }

    /// <summary>
    /// Removes a category from the task
    /// </summary>
    /// <param name="category">The category to remove</param>
    /// <returns>True if the category was removed, false if it didn't exist</returns>
    public bool RemoveCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return false;
        }

        var categoryToRemove = Categories.FirstOrDefault(c =>
            string.Equals(c, category.Trim(), StringComparison.OrdinalIgnoreCase));

        if (categoryToRemove != null)
        {
            Categories.Remove(categoryToRemove);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the task has the specified category
    /// </summary>
    /// <param name="category">The category to check for</param>
    /// <returns>True if the task has the category</returns>
    public bool HasCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return false;
        }

        return Categories.Any(c => string.Equals(c, category.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the task has any of the specified categories
    /// </summary>
    /// <param name="categories">The categories to check for</param>
    /// <returns>True if the task has any of the specified categories</returns>
    public bool HasAnyCategory(IEnumerable<string> categories)
    {
        if (categories == null)
        {
            return false;
        }

        return categories.Any(HasCategory);
    }

    /// <summary>
    /// Checks if the task has all of the specified categories
    /// </summary>
    /// <param name="categories">The categories to check for</param>
    /// <returns>True if the task has all of the specified categories</returns>
    public bool HasAllCategories(IEnumerable<string> categories)
    {
        if (categories == null)
        {
            return true;
        }

        return categories.All(HasCategory);
    }

    /// <summary>
    /// Clears all categories from the task
    /// </summary>
    public void ClearCategories()
    {
        Categories.Clear();
    }

    /// <summary>
    /// Updates the categories for the task, replacing all existing categories
    /// </summary>
    /// <param name="newCategories">The new categories to set</param>
    public void UpdateCategories(IEnumerable<string> newCategories)
    {
        Categories.Clear();
        if (newCategories != null)
        {
            foreach (var category in newCategories.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                AddCategory(category);
            }
        }
    }

    /// <summary>
    /// Gets a formatted string representation of all categories
    /// </summary>
    /// <returns>Comma-separated list of categories</returns>
    public string GetCategoriesString()
    {
        return Categories.Any() ? string.Join(", ", Categories) : "No categories";
    }

    /// <summary>
    /// Gets the number of categories assigned to this task
    /// </summary>
    public int CategoryCount => Categories.Count;
}
