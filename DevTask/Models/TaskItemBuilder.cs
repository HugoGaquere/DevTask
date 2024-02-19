using System;
using System.Text;

namespace DevTask.Models;

public class TaskItemBuilder
{
    private TaskType _taskType;
    private StringBuilder? _contentBuilder;
    private string? _filePath;
    private int _line;
    private DateTime? _time;

    public TaskItemBuilder SetType(TaskType type)
    {
        _taskType = type;
        return this;
    }
    
    public TaskItemBuilder AddContent(string content)
    {
        _contentBuilder ??= new StringBuilder();
        _contentBuilder.AppendLine(content);
        return this;
    }
    
    public TaskItemBuilder SetFilePath(string filePath)
    {
        _filePath = filePath;
        return this;
    }
    
    public TaskItemBuilder SetLine(int line)
    {
        _line = line;
        return this;
    }
    
    public TaskItemBuilder SetTime(DateTime time)
    {
        
        _time= time;
        return this;
    }
    
    public TaskItem Build()
    {
        if (_contentBuilder == null)
        {
            throw new ArgumentNullException(nameof(_contentBuilder), "Content cannot be null");
        }

        if (string.IsNullOrEmpty(_filePath))
        {
            throw new ArgumentNullException(nameof(_filePath), "FilePath cannot be null or empty");
        }
        
        if (_time == null)
        {
            throw new ArgumentNullException(nameof(_time), "Time cannot be null");
        }

        return new TaskItem(_taskType, _contentBuilder.ToString(), _filePath, _line, _time);
    }
}