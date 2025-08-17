using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using todo.app.Services;

namespace todo.app;

public enum TaskFilter
{
    All,
    Completed,
    Pending
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

        // Initialize the display
        UpdateFilterButtonStyles();
        UpdateStatistics();
    }

    private void OnAddTaskButtonClick(object? sender, RoutedEventArgs e)
    {
        var taskText = NewTaskTextBox.Text?.Trim();

        if (string.IsNullOrEmpty(taskText))
        {
            return; // Don't add empty tasks
        }

        try
        {
            // Use the service to add the task
            _todoService.AddTask(taskText);

            // Clear the input field
            NewTaskTextBox.Text = string.Empty;

            // Update the UI
            RefreshTaskList();
            UpdateStatistics();
        }
        catch (System.ArgumentException)
        {
            // Handle invalid input (though we check above, this is defensive)
            // Could show an error message to user in the future
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
            _ => _todoService.Tasks
        };

        // Add filtered tasks
        foreach (var task in tasksToShow)
        {
            // Create a horizontal stack panel for checkbox + text
            var stackPanel = new StackPanel
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
            checkBox.IsCheckedChanged += (sender, e) =>
            {
                // Use the service to toggle task completion
                _todoService.ToggleTaskCompletion(task.Id);

                // Update the visual state
                RefreshTaskList();
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
                StartEditingTask(task, stackPanel);
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
            deleteButton.Click += (sender, e) =>
            {
                // Use the service to remove the task
                _todoService.RemoveTask(task.Id);

                // Update the UI
                RefreshTaskList();
                UpdateStatistics();
            };

            // Add checkbox, text, and delete button to stack panel
            stackPanel.Children.Add(checkBox);
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(deleteButton);

            // Create border container
            var taskBorder = new Border
            {
                Background = task.IsCompleted ?
                    Avalonia.Media.Brushes.LightGreen :
                    Avalonia.Media.Brushes.LightGray,
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 0, 0, 5),
                CornerRadius = new Avalonia.CornerRadius(5),
                Child = stackPanel
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

    private void UpdateFilterButtonStyles()
    {
        // Reset all buttons to inactive style
        ShowAllButton.Background = Brushes.Gray;
        ShowCompletedButton.Background = Brushes.Gray;
        ShowPendingButton.Background = Brushes.Gray;

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
                FinishEditingTask(task, editBox.Text?.Trim() ?? string.Empty);
            }
            else if (e.Key == Avalonia.Input.Key.Escape)
            {
                CancelEditingTask();
            }
        };

        // Handle lost focus
        editBox.LostFocus += (sender, e) =>
        {
            FinishEditingTask(task, editBox.Text?.Trim() ?? string.Empty);
        };

        // Replace text block with edit box
        var textBlockIndex = taskPanel.Children.IndexOf(textBlock);
        taskPanel.Children.RemoveAt(textBlockIndex);
        taskPanel.Children.Insert(textBlockIndex, editBox);

        // Focus and select all text
        editBox.Focus();
        editBox.SelectAll();
    }

    private void FinishEditingTask(Models.TodoTask task, string newTitle)
    {
        if (!string.IsNullOrWhiteSpace(newTitle) && newTitle != task.Title)
        {
            try
            {
                _todoService.EditTask(task.Id, newTitle);
            }
            catch (ArgumentException)
            {
                // Handle invalid input - could show error message in future
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