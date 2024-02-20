using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DevTask.Models;

namespace DevTask.Services;

public class ScanProgressEventArgs(int totalFiles, int filesScanned) : EventArgs
{
    public int TotalFiles { get; } = totalFiles;
    public int FilesScanned { get; } = filesScanned;
}

public class ScanService
{
    public event EventHandler<ScanProgressEventArgs>? ScanProgressChanged;

    private static readonly HashSet<string> AllowedFileExtensions =
    [
        ".cs", // C#
        ".c", // C
        ".cpp", // C++
        ".h", // C++ Header
        ".js", // JavaScript
        ".ts", // TypeScript
        ".java", // Java
        ".kt", // Kotlin
        ".go", // Go
        ".rs", // Rust
        ".php", // PHP
        ".swift", // Swift
        ".scala", // Scala
        ".groovy", // Groovy
        ".dart" // Dart
    ];

    // TODO: The regex pattern is not flexible enough to handle different comment styles
    // Date: 13 / 02 / 2024
    private const string ContentPattern = @"\/\/\s*(?:(?i)todo|refactor|bug)?\s*:?\s*(.*)";
    private const string KeywordsPattern = @"\/\/\s*((?i)todo|refactor|bug)\s*:";
    private const string DatePattern = @"\/\/\s*(?i)date\s*:?\s*(\d{2}\s*\/\s*\d{2}\s*\/\s*\d{4})";

    public async Task<ScanResult> ScanProjectAsync(string projectPath)
    {
        var scanResult = await Task.Run(() => ScanFolder(projectPath));
        return scanResult;
    }

    private async Task<ScanResult> ScanFolder(string folderPath)
    {
        // Refactor: This method is too long and should be split into smaller methods
        // Date: 15 / 02 / 2024

        if (!Path.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"The folder {folderPath} does not exist.");
        }

        var stopwatch = Stopwatch.StartNew();

        var folderTaskItems = new ConcurrentBag<TaskItem>();
        var files = Directory.GetFiles(folderPath, searchPattern: "*", searchOption: SearchOption.AllDirectories);
        var allowedFiles = files.Where(IsFileExtensionAllowed).ToArray();

        var counter = 0;
        var tasks = allowedFiles.Select(filePath =>
        {
            return Task.Run(() =>
            {
                var currentCount = Interlocked.Increment(ref counter);
                ScanProgressChanged?.Invoke(this, new ScanProgressEventArgs(allowedFiles.Length, currentCount));
                var fileTaskItems = ScanFile(folderPath, filePath);
                foreach (var item in fileTaskItems)
                {
                    folderTaskItems.Add(item);
                }
            });
        });

        await Task.WhenAll(tasks);

        stopwatch.Stop();
        var duration = stopwatch.Elapsed;

        return new ScanResult(folderTaskItems, allowedFiles.Length, duration.Milliseconds);
    }

    private static IEnumerable<TaskItem> ScanFile(string folderPath, string filePath)
    {
        // TODO: The scan should be enough flexible to match comments when the date is not present
        // Date: 20 / 02 / 2024
        
        // Refactor: This method is too long and should be split into smaller methods
        // Date: 20/02/2024

        var keywordsRegex = new Regex(KeywordsPattern);
        var contentRegex = new Regex(ContentPattern);
        var dateRegex = new Regex(DatePattern);

        var relativeFilePath = filePath.Replace(folderPath, "").TrimStart('/', '\\');
        var results = new List<TaskItem>();

        using var reader = File.OpenText(filePath);
        TaskItemBuilder? taskItemBuilder = null;
        var lineCounter = 0;
        while (reader.ReadLine() is { } line)
        {
            lineCounter++;
            var keywordsMatch = keywordsRegex.Match(line);
            if (keywordsMatch.Success)
            {
                // Keyword found, start a new comment
                taskItemBuilder = new TaskItemBuilder();
                taskItemBuilder.SetType(ParseTaskType(keywordsMatch)).SetFilePath(relativeFilePath).SetLine(lineCounter);

                // Parse the comment content presents on the same line of the keyword
                var contentMatch = contentRegex.Match(line);
                if (contentMatch.Success) taskItemBuilder.AddContent(ParseTaskContent(contentMatch));
            }
            else if (taskItemBuilder != null)
            {
                // We are inside a comment
                // Check if it's date, if so this is the end of the comment
                var dateMatch = dateRegex.Match(line);
                if (dateMatch.Success)
                {
                    taskItemBuilder.SetTime(ParseTaskDate(dateMatch));
                    // Build the TaskItem and add it to the results
                    results.Add(taskItemBuilder.Build());
                    // Reset the TaskItemBuilder for next comments
                    taskItemBuilder = null;
                    continue;
                }

                // Since the line isn't a date, this must be a content line
                // If not, the comment format is wrong and not supported, abort it.
                var contentMatch = contentRegex.Match(line);
                if (!contentMatch.Success)
                {
                    // Reset the TaskItemBuilder for next comments
                    taskItemBuilder = null;
                    continue;
                }
                taskItemBuilder.AddContent(ParseTaskContent(contentMatch));
            }
        }

        return results;
    }

    private static TaskType ParseTaskType(Match match)
    {
        var taskTypeString = match.Groups[1].ToString();
        return Enum.Parse<TaskType>(taskTypeString, ignoreCase: true);
    }

    private static string ParseTaskContent(Match match)
    {
        return match.Groups[1].ToString().Trim();
    }

    private static DateTime ParseTaskDate(Match match)
    {
        var date = match.Groups[1].ToString();
        var dateArray = date.Split('/').Select(s => int.Parse(s.Trim())).ToList();
        return new DateTime(dateArray[2], dateArray[1], dateArray[0]);
    }
    
    private static bool IsFileExtensionAllowed(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return AllowedFileExtensions.Contains(extension);
    }

    public ScanResult GetMockScanResult() => new ScanResult(GetMockItems(), 4, 27);

    private static IEnumerable<TaskItem> GetMockItems() => new[]
    {
        new TaskItem(TaskType.Todo, "add feature", "src/main.py", 5, new DateTime(2024, 11, 10)),
        new TaskItem(TaskType.Todo, "add feature", "src/main.py", 7, new DateTime(2024, 11, 11)),
        new TaskItem(TaskType.Bug, "BufferOverflow", "src/compute.py", 50, new DateTime(2024, 11, 12)),
        new TaskItem(TaskType.Todo, "Check performances", "src/mandelbrot.py", 12, new DateTime(2024, 11, 12)),
        new TaskItem(TaskType.Refactor, "Refactor method", "src/matrix.py", 47, new DateTime(2023, 10, 25)),
    };
}