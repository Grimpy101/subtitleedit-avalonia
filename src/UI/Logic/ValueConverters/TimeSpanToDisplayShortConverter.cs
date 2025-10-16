﻿using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class TimeSpanToDisplayShortConverter : IValueConverter
{
    public static readonly TimeSpanToDisplayShortConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            if (Se.Settings.General.UseFrameMode)
            {
                return new TimeCode(ts).ToHHMMSSFF();
            }

            var result = new TimeCode(ts).ToShortDisplayString();
            return result;
        }
        
        if (Se.Settings.General.UseFrameMode)
        {
            return "00.00";
        }

        return "00,000";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var arr = s.Split(',', ':', '.', ';');
            if (arr.Length == 2 &&
                int.TryParse(arr[0], out var twoSec) &&
                int.TryParse(arr[1], out var twoMs))
            {
                return new TimeSpan(0, 0, 0, twoSec, twoMs, 0);
            }

            if (arr.Length == 3 &&
                int.TryParse(arr[0], out var threeMin) &&
                int.TryParse(arr[1], out var threeSec) &&
                int.TryParse(arr[2], out var threeMs))
            {
                return new TimeSpan(0, 0, threeMin, threeSec, threeMs, 0);
            }

            if (arr.Length == 4 &&
                int.TryParse(arr[0], out var fourHour) &&
                int.TryParse(arr[1], out var fourMin) &&
                int.TryParse(arr[2], out var fourSec) &&
                int.TryParse(arr[3], out var fourMs))
            {
                return new TimeSpan(0, fourHour, fourMin, fourSec, fourMs, 0);
            }
        }

        return TimeSpan.Zero;
    }
}