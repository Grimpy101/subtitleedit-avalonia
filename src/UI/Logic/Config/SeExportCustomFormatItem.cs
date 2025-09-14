﻿namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public class SeExportCustomFormatItem
{
    public string Name { get; set; }
    public string Extension { get; set; }
    public string FormatHeader { get; set; }
    public string FormatText { get; set; }
    public string FormatFooter { get; set; }
    public string FormatTimeCode { get; set; }
    public string FormatNewLine { get; set; }

    public SeExportCustomFormatItem()
    {
        Name = string.Empty;
        Extension = string.Empty;
        FormatHeader = string.Empty;
        FormatText = string.Empty;
        FormatFooter = string.Empty;
        FormatTimeCode = string.Empty;
        FormatNewLine = string.Empty;
    }
}
