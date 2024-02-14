using System;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using DevTask.Models;
using ReactiveUI;

namespace DevTask.ViewModels;

public class MainWindowViewModel : ViewModelBase
{

    private ScanResult? _scanResult;

    public ScanResult? ScanResult
    {
        get => _scanResult;
        set => this.RaiseAndSetIfChanged(ref _scanResult, value);
    }


    private TaskListViewModel _taskListViewModel;

    public TaskListViewModel TaskList
    {
        get => _taskListViewModel;
        set => this.RaiseAndSetIfChanged(ref _taskListViewModel, value);
    }

    private bool _isScanLoading = false;

    public bool IsScanLoading
    {
        get => _isScanLoading;
        set => this.RaiseAndSetIfChanged(ref _isScanLoading, value);
    }

    public ReactiveCommand<Unit, Unit> RunScanCommand { get; }

    private string? _folderPath;

    public String FolderPath
    {
        get => _folderPath ?? "Select project folder";
        set => this.RaiseAndSetIfChanged(ref _folderPath, value);
    }

    public MainWindowViewModel()
    {
        IObservable<bool> isFolderPathValid = this.WhenAnyValue(
            x => x.FolderPath,
            x => !string.IsNullOrWhiteSpace(x) && (x.Contains('/') || x.Contains('\\'))
        );

        RunScanCommand = ReactiveCommand.Create(() => { RunScan(); }, isFolderPathValid);
    }

    public async void RunScan()
    {
        IsScanLoading = true;
        
        var service = new ScanService();
        
        ScanResult = await service.ScanProjectAsync(FolderPath);
        TaskList = new TaskListViewModel(ScanResult.TaskItems);
        
        IsScanLoading = false;
    }

    public async Task OpenFolder()
    {
        // Clear last scan
        ScanResult = null;
        TaskList?.ListItems.Clear();
        
        try
        {
            var folder = await DoOpenFolderPickerAsync();
            if (folder is null) return;
            FolderPath = folder.Path.AbsolutePath;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private async Task<IStorageFolder?> DoOpenFolderPickerAsync()
    {
        // For learning purposes, we opted to directly get the reference
        // for StorageProvider APIs here inside the ViewModel. 

        // For your real-world apps, you should follow the MVVM principles
        // by making service classes and locating them with DI/IoC.

        // See IoCFileOps project for an example of how to accomplish this.
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");


        var folder = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Choose folder",
            AllowMultiple = false
        });

        return folder?.Count >= 1 ? folder[0] : null;
    }
}