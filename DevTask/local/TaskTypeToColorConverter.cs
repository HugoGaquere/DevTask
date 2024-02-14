using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DevTask.Models;

namespace DevTask.local;

public class TaskTypeToColorConverter : IValueConverter
{
    public static readonly TaskTypeToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskType sourceType && targetType.IsAssignableTo(typeof(IBrush)))
        {
            return sourceType switch
            {
                TaskType.Todo => Brushes.CornflowerBlue,
                TaskType.Refactor => Brushes.Plum,
                TaskType.Bug => Brushes.Orange,
                _ => Brushes.White
            };
        }
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), 
            BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}