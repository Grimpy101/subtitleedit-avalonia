﻿using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public class PaddleOcrDownloadService : IPaddleOcrDownloadService
{
    private readonly HttpClient _httpClient;
    public const string DownloadModelsUrl = "https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.3.0/PaddleOCR.PP-OCRv5.support.files.VideOCR.7z";
    public const string DownloadWindowsEngineCpuUrl = "https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.3.0/PaddleOCR-CPU-v1.3.0.7z";
    public const string DownloadWindowsEngineGpuUrl = "https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.3.0/PaddleOCR-GPU-v1.3.0-CUDA-11.8.7z";
    public const string DownloadLinuxEngineCpuUrl = "https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.3.0/PaddleOCR-CPU-v1.3.0-Linux.tar.xz";

    public PaddleOcrDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = DownloadModelsUrl;
        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadEngineCpu(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = DownloadWindowsEngineCpuUrl;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            url = DownloadLinuxEngineCpuUrl;
        }

        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadEngineGpu(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var url = DownloadWindowsEngineGpuUrl;
        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }
}