using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using todo.app.Models;

namespace todo.app;

public partial class MainWindow : Window
{
    private readonly List<TodoTask> _tasks;

    public MainWindow()
    {
        InitializeComponent();
        _tasks = new List<TodoTask>();

        // Wire up the button click event
        AddTaskButton.Click += OnAddTaskButtonClick;
    }

    private void OnAddTaskButtonClick(object? sender, RoutedEventArgs e)
    {
        var taskText = NewTaskTextBox.Text?.Trim();

        if (string.IsNullOrEmpty(taskText))
        {
            return; // Don't add empty tasks
        }

        // Create new task
        var newTask = new TodoTask(taskText);
        _tasks.Add(newTask);

        // Clear the input field
        NewTaskTextBox.Text = string.Empty;

        // Update the UI
        RefreshTaskList();
    }

    private void RefreshTaskList()
    {
        // Clear existing items (including sample tasks)
        TodoListPanel.Children.Clear();

        // Add all tasks
        foreach (var task in _tasks)
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
                if (checkBox.IsChecked == true)
                {
                    task.MarkAsCompleted();
                }
                else
                {
                    task.MarkAsIncomplete();
                }

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

            // Add checkbox and text to stack panel
            stackPanel.Children.Add(checkBox);
            stackPanel.Children.Add(textBlock);

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
}