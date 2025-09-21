﻿using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

internal static class SubtitleGridCopyPasteHelper
{
    internal static async Task Copy(Window window, List<SubtitleLineViewModel> selectedItems, SubtitleFormat subtitleFormat)
    {
        var subtitle = new Subtitle();
        foreach (var item in selectedItems)
        {
            subtitle.Paragraphs.Add(item.ToParagraph(subtitleFormat));
        }

        var text = subtitleFormat.ToText(subtitle, string.Empty);
        await ClipboardHelper.SetTextAsync(window, text);
    }

    internal static async Task Cut(Window window, ObservableCollection<SubtitleLineViewModel> subtitles, List<SubtitleLineViewModel> selectedItems, SubtitleFormat subtitleFormat)
    {
        var subtitle = new Subtitle();
        foreach (var item in selectedItems)
        {
            subtitle.Paragraphs.Add(item.ToParagraph(subtitleFormat));
        }

        var text = subtitleFormat.ToText(subtitle, string.Empty);
        await ClipboardHelper.SetTextAsync(window, text);

        foreach (var item in selectedItems)
        {
            subtitles.Remove(item);
        }
    }

    internal static async Task Paste(Window window, ObservableCollection<SubtitleLineViewModel> subtitles, int index, SubtitleFormat subtitleFormat)
    {
        var text = await ClipboardHelper.GetTextAsync(window);
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var addTimeMilliseconds = (double)0;
        if (subtitles.Count > 0)
        {
            addTimeMilliseconds = (double)subtitles[index].EndTime.TotalMilliseconds + Se.Settings.General.MinimumMillisecondsBetweenLines;
            index++;
        }

        var lines = text.SplitToLines();
        var subtitle = Subtitle.Parse(lines, subtitleFormat.Extension);
        if (subtitle.Paragraphs.Count > 0)
        {
            var firstSubtitleTime = subtitle.Paragraphs[0].StartTime.TotalMilliseconds;
            var adjust = addTimeMilliseconds - firstSubtitleTime;
            subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(adjust));
            LoadParagraphs(subtitles, index, subtitleFormat, subtitle);
            return;
        }

        foreach (SubtitleFormat item in SubtitleFormat.AllSubtitleFormats)
        {
            if (item.IsMine(lines, string.Empty))
            {
                item.LoadSubtitle(subtitle, lines, string.Empty);
                var firstSubtitleTime = subtitle.Paragraphs[0].StartTime.TotalMilliseconds;
                var adjust = addTimeMilliseconds - firstSubtitleTime;
                subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(adjust));
                LoadParagraphs(subtitles, index, subtitleFormat, subtitle);
                return;
            }
        }

        // fallback - plain text
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var p = new SubtitleLineViewModel
                {
                    StartTime = TimeSpan.FromMilliseconds(addTimeMilliseconds),
                    EndTime = TimeSpan.FromMilliseconds(addTimeMilliseconds + Se.Settings.General.NewEmptyDefaultMs),
                    Text = line.Trim()
                };
                subtitles.Insert(index, p);
                index++;
                addTimeMilliseconds += Se.Settings.General.NewEmptyDefaultMs + Se.Settings.General.MinimumMillisecondsBetweenLines;
            }
        }
    }

    private static void LoadParagraphs(ObservableCollection<SubtitleLineViewModel> subtitles, int index, SubtitleFormat subtitleFormat, Subtitle subtitle)
    {
        foreach (var p in subtitle.Paragraphs)
        {
            subtitles.Insert(index, new SubtitleLineViewModel(p, subtitleFormat));
            index++;
        }
    }
}
