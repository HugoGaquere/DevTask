using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using DevTask.Models;
using ReactiveUI;

namespace DevTask.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ScanService _scanService;
    private ScanResult? _scanResult;
    private TaskListViewModel? _taskListViewModel;
    private string? _folderPath;
    private bool _isScanLoading;
    private int _totalFiles;
    private int _scannedFiles;
    
    public ScanResult? ScanResult
    {
        get => _scanResult;
        set => this.RaiseAndSetIfChanged(ref _scanResult, value);
    }
    
    public TaskListViewModel? TaskList
    {
        get => _taskListViewModel;
        set => this.RaiseAndSetIfChanged(ref _taskListViewModel, value);
    }


    public bool IsScanLoading
    {
        get => _isScanLoading;
        set => this.RaiseAndSetIfChanged(ref _isScanLoading, value);
    }
    
    public string FolderPath
    {
        get => _folderPath ?? "Select project folder";
        set => this.RaiseAndSetIfChanged(ref _folderPath, value);
    }
    
    public int TotalFiles
    {
        get => _totalFiles;
        set => this.RaiseAndSetIfChanged(ref _totalFiles, value);
    }
    
    public int ScannedFiles
    {
        get => _scannedFiles;
        set => this.RaiseAndSetIfChanged(ref _scannedFiles, value);
    }
    
    public ReactiveCommand<Unit, Unit> RunScanCommand { get; }
    
    public MainWindowViewModel()
    {
        _scanService = new ScanService();
        _scanService.ScanProgressChanged += ScanService_ScanProgressChanged;
        
        var isFolderPathValid = this.WhenAnyValue(
            x => x.FolderPath,
            x => !string.IsNullOrWhiteSpace(x) && (x.Contains('/') || x.Contains('\\'))
        );
        RunScanCommand = ReactiveCommand.Create(() => { RunScan(); }, isFolderPathValid);
    }
    
    private void ScanService_ScanProgressChanged(object sender, ScanProgressEventArgs e)
    {
        TotalFiles = e.TotalFiles;
        ScannedFiles = e.FilesScanned;
    }

    public async void RunScan()
    {
        IsScanLoading = true;
        
        ScanResult = await _scanService.ScanProjectAsync(FolderPath);
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