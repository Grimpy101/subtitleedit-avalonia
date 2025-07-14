﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class NOcrCharacterHistoryWindow : Window
{
    private readonly NOcrCharacterHistoryViewModel _vm;

    public NOcrCharacterHistoryWindow(NOcrCharacterHistoryViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        Title = string.Empty;
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Controls
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var listView = MakeListView(vm);
        var detailsView = MakeListView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk);

        grid.Add(listView, 0, 0);
        grid.Add(detailsView, 0, 1);
        grid.Add(buttonBar, 1, 0, 2, 1);

        Content = grid;

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;

        Activated += delegate
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        };
    }

    private static Grid MakeListView(NOcrCharacterHistoryViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 20,
            Width = double.NaN,
        };

        return grid;
    }

    private static Grid MakeDetailsView(NOcrCharacterHistoryViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 20,
            Width = double.NaN,
        };

        vm.TextBoxNew = UiUtil.MakeTextBox(100, vm, nameof(vm.NewText));
        var image = new Image
        {
            Margin = new Thickness(5),
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            MinHeight = 30,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.CurrentBitmap)));


        var panelCurrentImage = new StackPanel
        {
            Background = new SolidColorBrush(Colors.LightGray),
            Children = { image },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(5, 2, 0, 5),
        };

        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsNewTextItalic));
        checkBoxItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.CurrentImage).WithBold(),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ResolutionAndTopMargin)),
                panelCurrentImage,
                vm.TextBoxNew,
                checkBoxItalic,
            },
        };

        var panelDrawControls = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.LinesToDraw).WithBold(),
            }
        };

        vm.NOcrDrawingCanvas.SetStrokeWidth(1);
        var borderDrawingCanvas = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Black),
            Child = vm.NOcrDrawingCanvas,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };

        var panelZoom = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5),
            Children =
            {
                UiUtil.MakeButton(vm.ZoomOutCommand, IconNames.MdiMinus),
                UiUtil.MakeButton(vm.ZoomInCommand, IconNames.MdiPlus),
                UiUtil.MakeLabel(string.Empty).WithMarginLeft(10).WithBindText(vm, nameof(vm.ZoomFactorInfo)),
                UiUtil.MakeLabel(Se.Language.Ocr.DrawMode).WithMarginLeft(10),
            }
        };

        var panelImage = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                panelZoom,
                borderDrawingCanvas,
            }
        };


        grid.Add(panelCurrent, 0, 0);
        grid.Add(panelDrawControls, 0, 1);
        grid.Add(panelImage, 0, 2);

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.KeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Title = _vm.Title;
    }
}
