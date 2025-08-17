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
            var taskBorder = new Border
            {
                Background = Avalonia.Media.Brushes.LightGray,
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 0, 0, 5),
                CornerRadius = new Avalonia.CornerRadius(5),
                Child = new TextBlock
                {
                    Text = task.Title,
                    FontSize = 14
                }
            };

            TodoListPanel.Children.Add(taskBorder);
        }
    }
}