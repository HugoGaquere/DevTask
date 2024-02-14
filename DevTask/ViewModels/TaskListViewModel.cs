using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DevTask.Models;

namespace DevTask.ViewModels;

public class TaskListViewModel : ViewModelBase
{
    public ObservableCollection<TaskItem> ListItems { get; }

    public TaskListViewModel(IEnumerable<TaskItem> items)
    {
        var sortedItems = items.OrderByDescending(i => i.DateTime);
        ListItems = new ObservableCollection<TaskItem>(sortedItems);
    }
}