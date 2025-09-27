using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class SubtitleLineViewModel : ObservableObject
{
    [ObservableProperty]
    private int _number;

    [ObservableProperty]
    private string? _bookmark;

    [ObservableProperty]
    private TimeSpan _startTime;

    [ObservableProperty]
    private TimeSpan _endTime;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private string _originalText;

    [ObservableProperty]
    private string _style;

    [ObservableProperty]
    private string _actor;

    [ObservableProperty]
    private int _layer;

    [ObservableProperty]
    private double _gap;

    public Paragraph? Paragraph { get; set; }
    public string Extra { get; set; }
    public string Language { get; set; }
    public string Region { get; set; }
    public Guid Id { get; set; }
    public bool IsDefault => Text == string.Empty && Number == 0 && Duration == TimeSpan.Zero && StartTime == TimeSpan.Zero;

    private bool _skipUpdate = false;
    private static Color _errorColor = Se.Settings.General.ErrorColor.FromHexToColor();

    public SubtitleLineViewModel()
    {
        Text = string.Empty;
        OriginalText = string.Empty;
        Extra = string.Empty;
        Language = string.Empty;
        Region = string.Empty;
        Style = string.Empty;
        Actor = string.Empty;
        Layer = 0;
        Id = Guid.NewGuid();
    }

    public SubtitleLineViewModel(SubtitleLineViewModel p, bool generateNewId = false)
    {
        Text = p.Text;
        OriginalText = p.OriginalText;
        StartTime = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds);
        EndTime = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds);
        UpdateDuration();
        Language = p.Language;
        Region = p.Region;
        Style = p.Style;
        Actor = p.Actor;
        Layer = p.Layer;
        Number = p.Number;
        Extra = p.Extra;

        Id = generateNewId ? Guid.NewGuid() : p.Id;

        if (p.Paragraph != null)
        {
            Paragraph = new Paragraph(p.Paragraph, generateNewId);
        }
    }

    public SubtitleLineViewModel(Paragraph paragraph, SubtitleFormat subtitleFormat)
    {        
        Text = paragraph.Text;
        OriginalText = string.Empty;
        Extra = paragraph.Extra;
        Language = paragraph.Language;
        Region = paragraph.Region;
        Style = paragraph.Style;
        Actor = paragraph.Actor;
        Layer = paragraph.Layer;
        Number = paragraph.Number;
        StartTime = TimeSpan.FromMilliseconds(paragraph.StartTime.TotalMilliseconds);
        EndTime = TimeSpan.FromMilliseconds(paragraph.EndTime.TotalMilliseconds);
        UpdateDuration();
        Id = Guid.TryParse(paragraph.Id, out var guid) ? guid : Guid.NewGuid();
        Paragraph = paragraph;

        if (subtitleFormat is AdvancedSubStationAlpha)
        {
            Style = paragraph.Extra;
        }
    }

    public Paragraph ToParagraph(SubtitleFormat? subtitleFormat = null)
    {
        var p = new Paragraph()
        {
            Number = Number,
            StartTime = new TimeCode(StartTime),
            EndTime = new TimeCode(EndTime),
            Text = Text,
            Actor = Actor,
            Style = Style,
            Language = Language,
            Region = Region,
            Layer = Layer,
            Bookmark = Bookmark,
        };

        if (subtitleFormat != null && subtitleFormat is AdvancedSubStationAlpha)
        {
            p.Extra = Style;
        }

        return p;
    }

    public Paragraph ToParagraphOriginal(SubtitleFormat? subtitleFormat = null)
    {
        var p = new Paragraph()
        {
            Number = Number,
            StartTime = new TimeCode(StartTime),
            EndTime = new TimeCode(EndTime),
            Text = OriginalText,
            Actor = Actor,
            Style = Style,
            Language = Language,
            Region = Region,
            Layer = Layer,
            Bookmark = Bookmark,
        };

        if (subtitleFormat != null && subtitleFormat is AdvancedSubStationAlpha)
        {
            p.Extra = Style;
        }

        return p;
    }

    public double CharactersPerSecond
    {
        get
        {
            if (string.IsNullOrEmpty(Text))
            {
                return 0;
            }

            if (Duration.TotalMilliseconds <= 1.0)
            {
                return 999.0;
            }

            return (double)HtmlUtil.RemoveHtmlTags(Text, true).CountCharacters(forCps: true) / Duration.TotalSeconds;
        }
    }

    public double WordsPerMinute // WPM
    {
        get
        {
            if (string.IsNullOrEmpty(Text))
            {
                return 0;
            }

            if (Duration.TotalMilliseconds <= 1.0)
            {
                return 999.0;
            }

            return 60.0 / Duration.TotalSeconds * Text.CountWords();
        }
    }

    public IBrush TextBackgroundBrush
    {
        get
        {
            if (Se.Settings.General.ColorTextTooLong && !string.IsNullOrEmpty(Text))
            {
                if (CharactersPerSecond > Se.Settings.General.SubtitleMaximumCharactersPerSeconds)
                {
                    return new SolidColorBrush(_errorColor);
                }

                var text = HtmlUtil.RemoveHtmlTags(Text, true);
                var lines = text.SplitToLines();
                foreach (var line in lines)
                {
                    if (line.Length > Se.Settings.General.SubtitleLineMaximumLength)
                    {
                        return new SolidColorBrush(_errorColor);
                    }
                }
            }

            return new SolidColorBrush(Colors.Transparent);
        }
    }

    public IBrush DurationBackgroundBrush
    {
        get
        {
            if (Se.Settings.General.ColorDurationTooShort &&
                Duration.TotalMilliseconds < Se.Settings.General.SubtitleMinimumDisplayMilliseconds)
            {
                return new SolidColorBrush(_errorColor);
            }

            if (Se.Settings.General.ColorDurationTooLong && Duration.TotalMilliseconds > Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
            {
                return new SolidColorBrush(_errorColor);
            }

            return new SolidColorBrush(Colors.Transparent);
        }
    }

    public IBrush CpsBackgroundBrush
    {
        get
        {
            if (Se.Settings.General.ColorDurationTooLong &&
                CharactersPerSecond > Se.Settings.General.SubtitleMaximumCharactersPerSeconds)
            {
                return new SolidColorBrush(_errorColor);
            }

            return new SolidColorBrush(Colors.Transparent);
        }
    }

    public IBrush WpmBackgroundBrush
    {
        get
        {
            if (Se.Settings.General.ColorDurationTooLong &&
                WordsPerMinute > Se.Settings.General.SubtitleMaximumWordsPerMinute)
            {
                return new SolidColorBrush(_errorColor);
            }

            return new SolidColorBrush(Colors.Transparent);
        }
    }

    partial void OnGapChanged(double value)
    {
        if (_skipUpdate)
        {
            return;
        }

        OnPropertyChanged(nameof(GapBackgroundBrush));
    }

    public IBrush GapBackgroundBrush
    {
        get
        {
            if (Se.Settings.General.ColorGapTooShort &&
                Gap < Se.Settings.General.MinimumMillisecondsBetweenLines)
            {
                return new SolidColorBrush(_errorColor);
            }

            return new SolidColorBrush(Colors.Transparent);
        }
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        if (_skipUpdate)
        {
            return;
        }

        _skipUpdate = true;
        EndTime = value + Duration;
        UpdateDuration();
        _skipUpdate = false;
    }

    partial void OnEndTimeChanged(TimeSpan value)
    {
        if (_skipUpdate)
        {
            return;
        }

        _skipUpdate = true;
        UpdateDuration();
        _skipUpdate = false;

        OnPropertyChanged(nameof(CharactersPerSecond));
        OnPropertyChanged(nameof(TextBackgroundBrush));
        OnPropertyChanged(nameof(DurationBackgroundBrush));
        OnPropertyChanged(nameof(CpsBackgroundBrush));
        OnPropertyChanged(nameof(WordsPerMinute));
        OnPropertyChanged(nameof(WpmBackgroundBrush));
    }

    partial void OnDurationChanged(TimeSpan value)
    {
        if (_skipUpdate)
        {
            return;
        }

        var newEndTime = StartTime + value;
        if (Math.Abs(newEndTime.TotalMilliseconds - EndTime.TotalMilliseconds) > 0.001)
        {
            EndTime = newEndTime;

            OnPropertyChanged(nameof(CharactersPerSecond));
            OnPropertyChanged(nameof(TextBackgroundBrush));
            OnPropertyChanged(nameof(DurationBackgroundBrush));
            OnPropertyChanged(nameof(CpsBackgroundBrush));
            OnPropertyChanged(nameof(WordsPerMinute));
            OnPropertyChanged(nameof(WpmBackgroundBrush));
        }
    }

    internal void UpdateDuration()
    {
        var newDuration = EndTime - StartTime;
        if (Math.Abs(newDuration.TotalMilliseconds - Duration.TotalMilliseconds) > 0.001)
        {
            Duration = EndTime - StartTime;

            OnPropertyChanged(nameof(CharactersPerSecond));
            OnPropertyChanged(nameof(TextBackgroundBrush));
            OnPropertyChanged(nameof(DurationBackgroundBrush));
        }
    }

    partial void OnTextChanged(string value)
    {
        OnPropertyChanged(nameof(CharactersPerSecond));
        OnPropertyChanged(nameof(TextBackgroundBrush));
        OnPropertyChanged(nameof(CpsBackgroundBrush));
        OnPropertyChanged(nameof(WordsPerMinute));
        OnPropertyChanged(nameof(WpmBackgroundBrush));
    }

    internal void SetStartTimeOnly(TimeSpan timeSpan)
    {
        _skipUpdate = true;
        StartTime = timeSpan;
        _skipUpdate = false;

        UpdateDuration();
    }

    internal void Adjust(double factor, double adjustmentInSeconds)
    {
        if (StartTime.IsMaxTime())
        {
            return;
        }

        SetStartTimeOnly(TimeSpan.FromMilliseconds(StartTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit));
        EndTime = TimeSpan.FromMilliseconds(EndTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit);
    }

    internal double GetCharactersPerSecond()
    {
        if (Duration.TotalMilliseconds < 1)
        {
            return 999;
        }

        return (double)Text.CountCharacters(true) / Duration.TotalSeconds;
    }
}