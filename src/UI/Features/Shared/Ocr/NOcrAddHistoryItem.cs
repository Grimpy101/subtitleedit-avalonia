﻿using Nikse.SubtitleEdit.Logic.Ocr;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class NOcrAddHistoryItem
{
    public NOcrChar NOcrChar { get; set; }
    public NikseBitmap2? Bitmap { get; set; }
    public int LineIndex { get; set; }
    public DateTime DateTime { get; set; }

    public NOcrAddHistoryItem(NOcrChar nOcrChar, NikseBitmap2? bitmap, int lineIndex)
    {
        NOcrChar = nOcrChar;
        Bitmap = bitmap;
        DateTime = DateTime.Now;
        LineIndex = lineIndex;
    }
}
