using System.Collections.Generic;
using System.Linq;

namespace DevTask.Models;

public class ScanResult(IEnumerable<TaskItem> taskItems, int numberFiles, int scanTime)
{
    public IEnumerable<TaskItem> TaskItems { get; } = taskItems;
    public int NumberOfTasks { get; } = taskItems.Count();
    public int NumberFiles { get; } = numberFiles;
    public int ScanTimeMilli { get; } = scanTime;

}