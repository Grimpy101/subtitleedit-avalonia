﻿using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class DoubleToTwoDecimalConverter : IValueConverter
{
    public static readonly DoubleToOneDecimalConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return d.ToString("0.00", culture); // rounded automatically
        }

        return "0.00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && double.TryParse(s, NumberStyles.Float, culture, out var result))
        {
            return result;
        }

        return 0.0;
    }
}
