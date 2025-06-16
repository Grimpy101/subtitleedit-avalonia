﻿using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public interface IOcrSubtitle
{
    int Count { get; }
    SKBitmap GetBitmap(int index);
    TimeSpan GetStartTime(int index);
    TimeSpan GetEndTime(int index);
    List<Shared.Ocr.OcrSubtitleItem> MakeOcrSubtitleItems();
}