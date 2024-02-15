using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DynamicData;

namespace DevTask.Models;

public class ScanProgressEventArgs(int totalFiles, int filesScanned) : EventArgs
{
    public int TotalFiles { get; } = totalFiles;
    public int FilesScanned { get; } = filesScanned;
}

public class ScanService
{
    public event EventHandler<ScanProgressEventArgs> ScanProgressChanged;

    private readonly Regex _regex;

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

    public ScanService()
    {
        // TODO: The regex pattern is not flexible enough to handle different comment styles
        // Date: 13 / 02 / 2024
        // TODO: The regex should be enough flexible to match when the date is not present
        // Date: 14 / 02 / 2024
        const string pattern =
            @"\/\/\s*((?i)todo|refactor|bug)\s*:\s*([\s\S]*?)\n\s*\/\/\s*Date\s*:\s*(\d{2}\s*\/\s*\d{2}\s*\/\s*\d{4})";
        _regex = new Regex(pattern);
    }

    public async Task<ScanResult> ScanProjectAsync(string projectPath)
    {
        var scanResult = await Task.Run(() => ScanFolder(projectPath));
        return scanResult;
    }

    private ScanResult ScanFolder(string folderPath)
    {
        if (!Path.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"The folder {folderPath} does not exist.");
        }

        IList<TaskItem> folderTaskItems = [];

        // Todo: Use multithreading to scan files in parallel
        // Date: 14 / 02 / 2024

        var stopwatch = Stopwatch.StartNew();

        var files = Directory.GetFiles(folderPath, searchPattern: "*", searchOption: SearchOption.AllDirectories);
        var allowedFiles = files.Where(IsFileExtensionAllowed).ToArray();
        for (var i = 0; i < allowedFiles.Length; i++)
        {
            ScanProgressChanged?.Invoke(this, new ScanProgressEventArgs(allowedFiles.Length, i + 1));
            var fileTaskItems = ScanFile(folderPath, allowedFiles[i]);
            folderTaskItems.AddRange(fileTaskItems);
        }

        stopwatch.Stop();
        var duration = stopwatch.Elapsed;

        return new ScanResult(folderTaskItems, allowedFiles.Length, duration.Milliseconds);
    }

    private IEnumerable<TaskItem> ScanFile(string folderPath, string filePath)
    {
        // TODO: There is a high memory consumption due to the reader.ReadToEnd() method,
        // which loads the entire file into memory.
        // A memory-efficient approach would be to read and process the file line by line
        // or chunk by chunk.
        // Date: 13 / 02 / 2024

        var relativeFilePath = filePath.Replace(folderPath, "").TrimStart('/', '\\');

        IList<TaskItem> results = new List<TaskItem>();
        using (var reader = File.OpenText(filePath))
        {
            var fileContent = reader.ReadToEnd();
            var matchCollection = _regex.Matches(fileContent);
            foreach (Match match in matchCollection)
            {
                var taskItem = CreateTaskItem(match, fileContent, relativeFilePath);
                results.Add(taskItem);
            }
        }

        return results;
    }

    private static TaskItem CreateTaskItem(Match match, string fileContent, string relativeFilePath)
    {
        var lineNumber = ParseTaskLineNumber(match, fileContent);
        var taskType = ParseTaskType(match);
        var content = ParseTaskContent(match);
        var dateTime = ParseTaskDate(match);
        return new TaskItem(taskType, content, relativeFilePath, lineNumber, dateTime);
    }

    private static int ParseTaskLineNumber(Match match, string fileContent)
    {
        return fileContent[..match.Index].Count(c => c == '\n') + 1;
    }

    private static TaskType ParseTaskType(Match match)
    {
        var taskTypeString = match.Groups[1].ToString();
        var taskType = Enum.Parse<TaskType>(taskTypeString, ignoreCase: true);
        return taskType;
    }

    private static string ParseTaskContent(Match match)
    {
        var content = match.Groups[2].ToString().Trim();
        // Remove the leading '//' and extra spaces of multiline comments 
        var cleanContent = Regex.Replace(content, @"\s*\/\/\s", "\n");
        return cleanContent;
    }

    private static DateTime ParseTaskDate(Match match)
    {
        var date = match.Groups[3].ToString();
        var dateArray = date.Split('/').Select(s => int.Parse(s.Trim())).ToList();
        return new DateTime(dateArray[2], dateArray[1], dateArray[0]);
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

    private bool IsFileExtensionAllowed(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return AllowedFileExtensions.Contains(extension);
    }
}