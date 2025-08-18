using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using todo.app.Services;
using todo.app.Models;

namespace todo.app;

public enum TaskFilter
{
    All,
    Completed,
    Pending,
    Overdue,
    DueToday
}

public partial class MainWindow : Window
{
    private readonly TodoService _todoService;
    private TaskFilter _currentFilter = TaskFilter.All;

    public MainWindow()
    {
        InitializeComponent();
        _todoService = new TodoService();

        // Wire up the button click events
        AddTaskButton.Click += OnAddTaskButtonClick;
        ShowAllButton.Click += OnShowAllButtonClick;
        ShowCompletedButton.Click += OnShowCompletedButtonClick;
        ShowPendingButton.Click += OnShowPendingButtonClick;
        ShowOverdueButton.Click += OnShowOverdueButtonClick;
        ShowDueTodayButton.Click += OnShowDueTodayButtonClick;
        ClearDueDateButton.Click += OnClearDueDateButtonClick;

        // Initialize the display
        UpdateFilterButtonStyles();
        UpdateStatistics();

        // Load saved tasks
        _ = LoadTasksAsync();
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            await _todoService.LoadTasksAsync();
            RefreshTaskList();
            UpdateStatistics();
        }
        catch (Exception)
        {
            // Handle loading errors gracefully - could show user message in future
        }
    }

    private async void OnAddTaskButtonClick(object? sender, RoutedEventArgs e)
    {
        var taskText = NewTaskTextBox.Text?.Trim();

        if (string.IsNullOrEmpty(taskText))
        {
            return; // Don't add empty tasks
        }

        try
        {
            // Get priority from combo box
            var priority = PriorityComboBox.SelectedIndex switch
            {
                0 => TaskPriority.Low,
                1 => TaskPriority.Medium,
                2 => TaskPriority.High,
                _ => TaskPriority.Medium
            };

            // Get due date from date picker
            var dueDate = DueDatePicker.SelectedDate?.DateTime;

            // Use the async service to add the task with priority and due date
            if (dueDate.HasValue)
            {
                await _todoService.AddTaskAsync(taskText, priority, dueDate.Value);
            }
            else
            {
                await _todoService.AddTaskAsync(taskText, priority);
            }

            // Clear the input fields
            NewTaskTextBox.Text = string.Empty;
            PriorityComboBox.SelectedIndex = 1; // Reset to Medium
            DueDatePicker.SelectedDate = null;

            // Update the UI
            RefreshTaskList();
            UpdateStatistics();
        }
        catch (System.ArgumentException)
        {
            // Handle invalid input (though we check above, this is defensive)
            // Could show an error message to user in the future
        }
        catch (Exception)
        {
            // Handle persistence errors gracefully
        }
    }

    private void RefreshTaskList()
    {
        // Clear existing items
        TodoListPanel.Children.Clear();

        // Get filtered tasks from the service
        var tasksToShow = _currentFilter switch
        {
            TaskFilter.Completed => _todoService.GetCompletedTasks(),
            TaskFilter.Pending => _todoService.GetPendingTasks(),
            TaskFilter.Overdue => _todoService.GetOverdueTasks(),
            TaskFilter.DueToday => _todoService.GetTasksDueToday(),
            _ => _todoService.Tasks
        };

        // Add filtered tasks
        foreach (var task in tasksToShow)
        {
            // Create a vertical stack panel for task content
            var taskContent = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Spacing = 5
            };

            // Create main row with checkbox, text, and delete button
            var mainRow = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 10
            };

            // Create checkbox for task completion
            var checkBox = new CheckBox
            {
                IsChecked = task.IsCompleted,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            // Handle checkbox change event
            checkBox.IsCheckedChanged += async (sender, e) =>
            {
                try
                {
                    // Use the async service to toggle task completion (auto-saves)
                    await _todoService.ToggleTaskCompletionAsync(task.Id);

                    // Update the visual state
                    RefreshTaskList();
                    UpdateStatistics();
                }
                catch (Exception)
                {
                    // Handle persistence errors gracefully
                }
            };

            // Create text block with strikethrough if completed
            var textBlock = new TextBlock
            {
                Text = task.Title,
                FontSize = 14,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            // Apply strikethrough style if task is completed
            if (task.IsCompleted)
            {
                textBlock.TextDecorations = Avalonia.Media.TextDecorations.Strikethrough;
                textBlock.Foreground = Avalonia.Media.Brushes.Gray;
            }

            // Add double-click event for editing
            textBlock.DoubleTapped += (sender, e) =>
            {
                StartEditingTask(task, mainRow);
            };

            // Create delete button
            var deleteButton = new Button
            {
                Content = "🗑️",
                FontSize = 12,
                Width = 30,
                Height = 30,
                Background = Avalonia.Media.Brushes.LightCoral,
                Foreground = Avalonia.Media.Brushes.White,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            // Handle delete button click
            deleteButton.Click += async (sender, e) =>
            {
                try
                {
                    // Use the async service to remove the task (auto-saves)
                    await _todoService.RemoveTaskAsync(task.Id);

                    // Update the UI
                    RefreshTaskList();
                    UpdateStatistics();
                }
                catch (Exception)
                {
                    // Handle persistence errors gracefully
                }
            };

            // Add checkbox, text, and delete button to main row
            mainRow.Children.Add(checkBox);
            mainRow.Children.Add(textBlock);
            mainRow.Children.Add(deleteButton);

            // Add main row to task content
            taskContent.Children.Add(mainRow);

            // Create metadata row for priority and due date
            var metadataRow = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 15,
                Margin = new Avalonia.Thickness(30, 0, 0, 0) // Indent to align with text
            };

            // Add priority badge
            var priorityBadge = new Border
            {
                Background = task.Priority switch
                {
                    TaskPriority.High => Avalonia.Media.Brushes.Red,
                    TaskPriority.Medium => Avalonia.Media.Brushes.Orange,
                    TaskPriority.Low => Avalonia.Media.Brushes.Green,
                    _ => Avalonia.Media.Brushes.Gray
                },
                CornerRadius = new Avalonia.CornerRadius(3),
                Padding = new Avalonia.Thickness(5, 2),
                Child = new TextBlock
                {
                    Text = $"Priority: {task.Priority}",
                    FontSize = 12,
                    Foreground = Avalonia.Media.Brushes.White
                }
            };

            metadataRow.Children.Add(priorityBadge);

            // Add due date info if present
            if (task.DueDate.HasValue)
            {
                var dueDateText = task.DueDate.Value.Date == DateTime.Today
                    ? "Due Today"
                    : task.IsOverdue
                        ? $"Overdue ({task.DueDate.Value:MMM dd})"
                        : $"Due {task.DueDate.Value:MMM dd}";

                var dueDateBadge = new Border
                {
                    Background = task.IsOverdue
                        ? Avalonia.Media.Brushes.DarkRed
                        : task.IsDueToday
                            ? Avalonia.Media.Brushes.DarkOrange
                            : Avalonia.Media.Brushes.DarkBlue,
                    CornerRadius = new Avalonia.CornerRadius(3),
                    Padding = new Avalonia.Thickness(5, 2),
                    Child = new TextBlock
                    {
                        Text = dueDateText,
                        FontSize = 12,
                        Foreground = Avalonia.Media.Brushes.White
                    }
                };

                metadataRow.Children.Add(dueDateBadge);
            }

            // Add metadata row to task content
            taskContent.Children.Add(metadataRow);

            // Determine border color based on task status
            var borderBrush = task.IsCompleted
                ? Avalonia.Media.Brushes.LightGreen
                : task.IsOverdue
                    ? Avalonia.Media.Brushes.LightCoral
                    : task.IsDueToday
                        ? Avalonia.Media.Brushes.LightGoldenrodYellow
                        : Avalonia.Media.Brushes.LightGray;

            // Create border container
            var taskBorder = new Border
            {
                Background = borderBrush,
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 0, 0, 5),
                CornerRadius = new Avalonia.CornerRadius(5),
                Child = taskContent
            };

            TodoListPanel.Children.Add(taskBorder);
        }
    }

    private void UpdateStatistics()
    {
        // Update the statistics labels with current counts
        TotalCountLabel.Text = _todoService.TotalTaskCount.ToString();
        CompletedCountLabel.Text = _todoService.CompletedTaskCount.ToString();
        PendingCountLabel.Text = _todoService.PendingTaskCount.ToString();
        OverdueCountLabel.Text = _todoService.GetOverdueTasks().Count().ToString();
    }

    private void OnShowAllButtonClick(object? sender, RoutedEventArgs e)
    {
        _currentFilter = TaskFilter.All;
        UpdateFilterButtonStyles();
        RefreshTaskList();
    }

    private void OnShowCompletedButtonClick(object? sender, RoutedEventArgs e)
    {
        _currentFilter = TaskFilter.Completed;
        UpdateFilterButtonStyles();
        RefreshTaskList();
    }

    private void OnShowPendingButtonClick(object? sender, RoutedEventArgs e)
    {
        _currentFilter = TaskFilter.Pending;
        UpdateFilterButtonStyles();
        RefreshTaskList();
    }

    private void OnShowOverdueButtonClick(object? sender, RoutedEventArgs e)
    {
        _currentFilter = TaskFilter.Overdue;
        UpdateFilterButtonStyles();
        RefreshTaskList();
    }

    private void OnShowDueTodayButtonClick(object? sender, RoutedEventArgs e)
    {
        _currentFilter = TaskFilter.DueToday;
        UpdateFilterButtonStyles();
        RefreshTaskList();
    }

    private void OnClearDueDateButtonClick(object? sender, RoutedEventArgs e)
    {
        DueDatePicker.SelectedDate = null;
    }

    private void UpdateFilterButtonStyles()
    {
        // Reset all buttons to inactive style
        ShowAllButton.Background = Brushes.Gray;
        ShowCompletedButton.Background = Brushes.Gray;
        ShowPendingButton.Background = Brushes.Gray;
        ShowOverdueButton.Background = Brushes.Gray;
        ShowDueTodayButton.Background = Brushes.Gray;

        // Highlight the active filter button
        switch (_currentFilter)
        {
            case TaskFilter.All:
                ShowAllButton.Background = Brushes.DarkBlue;
                break;
            case TaskFilter.Completed:
                ShowCompletedButton.Background = Brushes.Green;
                break;
            case TaskFilter.Pending:
                ShowPendingButton.Background = Brushes.Orange;
                break;
            case TaskFilter.Overdue:
                ShowOverdueButton.Background = Brushes.Red;
                break;
            case TaskFilter.DueToday:
                ShowDueTodayButton.Background = Brushes.DarkOrange;
                break;
        }
    }

    private void StartEditingTask(Models.TodoTask task, StackPanel taskPanel)
    {
        // Find the text block in the stack panel
        var textBlock = taskPanel.Children.OfType<TextBlock>().FirstOrDefault();
        if (textBlock == null) return;

        // Create an edit text box
        var editBox = new TextBox
        {
            Text = task.Title,
            FontSize = 14,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            MinWidth = 200
        };

        // Handle key events
        editBox.KeyDown += (sender, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                _ = FinishEditingTaskAsync(task, editBox.Text?.Trim() ?? string.Empty);
            }
            else if (e.Key == Avalonia.Input.Key.Escape)
            {
                CancelEditingTask();
            }
        };

        // Handle lost focus
        editBox.LostFocus += (sender, e) =>
        {
            _ = FinishEditingTaskAsync(task, editBox.Text?.Trim() ?? string.Empty);
        };

        // Replace text block with edit box
        var textBlockIndex = taskPanel.Children.IndexOf(textBlock);
        taskPanel.Children.RemoveAt(textBlockIndex);
        taskPanel.Children.Insert(textBlockIndex, editBox);

        // Focus and select all text
        editBox.Focus();
        editBox.SelectAll();
    }

    private async Task FinishEditingTaskAsync(Models.TodoTask task, string newTitle)
    {
        if (!string.IsNullOrWhiteSpace(newTitle) && newTitle != task.Title)
        {
            try
            {
                await _todoService.EditTaskAsync(task.Id, newTitle);
            }
            catch (ArgumentException)
            {
                // Handle invalid input - could show error message in future
            }
            catch (Exception)
            {
                // Handle persistence errors gracefully
            }
        }

        // Refresh the entire list to restore normal view
        RefreshTaskList();
        UpdateStatistics();
    }

    private void CancelEditingTask()
    {
        // Simply refresh to restore normal view
        RefreshTaskList();
    }
}