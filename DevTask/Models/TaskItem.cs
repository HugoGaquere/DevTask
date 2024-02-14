using System;

namespace DevTask.Models;

public enum TaskType
{
    Todo,
    Bug,
    Refactor,
}

public class TaskItem(TaskType taskType, string content, string filePath, int line, DateTime? dateTime)
{
    public TaskType Type { get; } = taskType;
    public string Content { get; } = content;
    public string FilePath { get; } = filePath;
    public int Line { get; } = line;

    public DateTime? DateTime { get; } = dateTime;
}