using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper.Engines;
using Nikse.SubtitleEdit.Logic.Download;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public partial class DownloadWhisperModelsViewModel : ObservableObject
{

    [ObservableProperty] private ObservableCollection<WhisperModelDisplay> _models;
    [ObservableProperty] private WhisperModelDisplay? _selectedModel;

    [ObservableProperty] private double _progressOpacity;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _error;

    public DownloadWhisperModelsWindow? Window { get; set; }
    public bool OkPressed { get; internal set; }

    private IWhisperDownloadService _whisperDownloadService;
    private IWhisperEngine? _whisperEngine;

    private Task? _downloadTask;
    private readonly List<string> _downloadUrls = new();
    private int _downloadIndex;
    private string _downloadFileName = string.Empty;
    private WhisperModel? _downloadModel;

    private const string TemporaryFileExtension = ".$$$";
    private readonly Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public DownloadWhisperModelsViewModel(IWhisperDownloadService whisperDownloadService)
    {
        Models = new ObservableCollection<WhisperModelDisplay>();
        SelectedModel = Models.FirstOrDefault();

        _whisperDownloadService = whisperDownloadService;

        _cancellationTokenSource = new CancellationTokenSource();

        ProgressText = "Starting...";
        Error = string.Empty;

        _timer = new Timer(500);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    public void SetModels(ObservableCollection<WhisperModelDisplay> models, IWhisperEngine whisperEngine, WhisperModelDisplay? whisperModel)
    {
        _whisperEngine = whisperEngine;

        foreach (var model in models)
        {
            Models.Add(model);
        }

        if (whisperModel != null)
        {
            SelectedModel = whisperModel;
        }
        else if (models.Count > 0)
        {
            SelectedModel = models[0];
        }
    }

    private readonly Lock _lockObj = new();

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            _timer.Stop();

            if (_downloadTask is { IsCompleted: true })
            {
                CompleteDownload();

                _downloadIndex++;
                if (_downloadIndex < _downloadUrls.Count)
                {
                    _downloadFileName = GetDownloadFileName(_downloadModel!, _downloadUrls[_downloadIndex]);
                    _downloadTask = _whisperDownloadService.DownloadFile(_downloadUrls[_downloadIndex], _downloadFileName, MakeDownloadProgress(), _cancellationTokenSource.Token);
                    ProgressValue = 0;
                    _timer.Start();

                    return;
                }

                _downloadTask = null;

                OkPressed = true;
                Close();

                return;
            }

            if (_downloadTask is { IsFaulted: true })
            {
                var ex = _downloadTask.Exception?.InnerException ?? _downloadTask.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Cancel();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }

                return;
            }

            _timer.Start();
        }
    }

    private void CompleteDownload()
    {
        var downloadFileName = _downloadFileName;
        if (string.IsNullOrEmpty(downloadFileName) || !File.Exists(downloadFileName))
        {
            return;
        }

        var fileInfo = new FileInfo(downloadFileName);
        if (fileInfo.Length == 0)
        {
            try
            {
                File.Delete(downloadFileName);
            }
            catch
            {
                // ignore
            }

            return;
        }

        if (fileInfo.Length < 50)
        {
            var text = FileUtil.ReadAllTextShared(downloadFileName, Encoding.UTF8);
            if (text.StartsWith("Entry not found", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Delete(downloadFileName);
                }
                catch
                {
                    // ignore
                }

                return;
            }

            if (text.Contains("Invalid username or password."))
            {
                throw new Exception("Unable to download file - Invalid username or password! (Perhaps file has a new location)");
            }
        }

        var newFileName = downloadFileName.Replace(TemporaryFileExtension, string.Empty);

        if (File.Exists(newFileName))
        {
            File.Delete(newFileName);
        }

        File.Move(downloadFileName, newFileName);
        _downloadFileName = string.Empty;
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        _timer.Stop();
        OkPressed = false;
        Close();
    }

    [RelayCommand]
    private async Task OpenModelFolder()
    {
        if (_whisperEngine == null)
        {
            return;
        }

        var folder = _whisperEngine.GetAndCreateWhisperModelFolder(_selectedModel?.Model);
        var dirInfo = new DirectoryInfo(folder);
        await Window!.Launcher.LaunchDirectoryInfoAsync(dirInfo);
    }

    [RelayCommand]
    private void Download()
    {
        if (SelectedModel is not { } model)
        {
            return;
        }

        _downloadUrls.Clear();
        _downloadUrls.AddRange(model.Model.Urls);
        _downloadIndex = 0;
        _downloadModel = model.Model;
        _downloadFileName = GetDownloadFileName(model.Model, _downloadUrls[_downloadIndex]);
        _downloadTask = _whisperDownloadService.DownloadFile(_downloadUrls[_downloadIndex], _downloadFileName, MakeDownloadProgress(), _cancellationTokenSource.Token);
        _timer.Interval = 500;
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();

        ProgressOpacity = 1;
    }

    private Progress<float> MakeDownloadProgress()
    {
        return new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = $"Downloading... {pctString}%";
        });
    }

    private string GetDownloadFileName(WhisperModel whisperModel, string url)
    {
        var fileName = _whisperEngine!.GetWhisperModelDownloadFileName(whisperModel, url);
        return fileName + TemporaryFileExtension;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        Cancel();
    }
}