﻿using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Assa;

public class AssaPropertiesWindow : Window
{
    public AssaPropertiesWindow(AssaPropertiesViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(MakeScriptView(vm), 0);
        grid.Add(MakeResolutionView(vm), 1);
        grid.Add(MakeOptionsView(vm), 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeScriptView(AssaPropertiesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel("Script").WithBold().WithMarginBottom(10);

        var labelTitle = UiUtil.MakeLabel("Title:");
        var textBoxTitle = UiUtil.MakeTextBox(200, vm, nameof(vm.ScriptTitle));

        var labelOriginalScript = UiUtil.MakeLabel("Original Script:");
        var textBoxOriginalScript = UiUtil.MakeTextBox(200, vm, nameof(vm.OriginalScript));

        var labelTranslation = UiUtil.MakeLabel("Translation:");
        var textBoxTranslation = UiUtil.MakeTextBox(200, vm, nameof(vm.Translation));

        var labelEditing = UiUtil.MakeLabel("Editing:");
        var textBoxEditing = UiUtil.MakeTextBox(200, vm, nameof(vm.Editing));

        var labelTiming = UiUtil.MakeLabel("Timing:");
        var textBoxTiming = UiUtil.MakeTextBox(200, vm, nameof(vm.Timing));

        var labelSyncPoint = UiUtil.MakeLabel("Sync Point:");
        var textBoxSyncPoint = UiUtil.MakeTextBox(200, vm, nameof(vm.SyncPoint));

        var labelUpdatedBy = UiUtil.MakeLabel("Updated By:");
        var textBoxUpdatedBy = UiUtil.MakeTextBox(200, vm, nameof(vm.UpdatedBy));

        var labelUpdateDetails = UiUtil.MakeLabel("Update Details:");
        var textBoxUpdateDetails = UiUtil.MakeTextBox(200, vm, nameof(vm.UpdateDetails));

        grid.Add(label, 0, 0, 1, 2);
        grid.Add(labelTitle, 1, 0);
        grid.Add(textBoxTitle, 1, 1);
        grid.Add(labelOriginalScript, 2, 0);
        grid.Add(textBoxOriginalScript, 2, 1);
        grid.Add(labelTranslation, 3, 0);
        grid.Add(textBoxTranslation, 3, 1);
        grid.Add(labelEditing, 4, 0);
        grid.Add(textBoxEditing, 4, 1);
        grid.Add(labelTiming, 5, 0);
        grid.Add(textBoxTiming, 5, 1);
        grid.Add(labelSyncPoint, 6, 0);
        grid.Add(textBoxSyncPoint, 6, 1);
        grid.Add(labelUpdatedBy, 7, 0);
        grid.Add(textBoxUpdatedBy, 7, 1);
        grid.Add(labelUpdateDetails, 8, 0);
        grid.Add(textBoxUpdateDetails, 8, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeResolutionView(AssaPropertiesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
        };

        var label = UiUtil.MakeLabel("Resolution").WithBold().WithMarginBottom(10);

        var labelVideoResolution = UiUtil.MakeLabel("Video Resolution");
        var numericUpDownWidth = UiUtil.MakeNumericUpDownInt(0, 10000, 120, vm, nameof(vm.VideoWidth));
        var labelX = UiUtil.MakeLabel("x");
        var numericUpDownHeight = UiUtil.MakeNumericUpDownInt(0, 10000, 120, vm, nameof(vm.VideoHeight));
        var buttonBrowseResolution = UiUtil.MakeButtonBrowse(vm.BrowseResolutionCommand);
        var buttonFromCurrentVideo = UiUtil.MakeButton("From Current Video", vm.GetResolutionFromCurrentVideoCommand);

        grid.Add(label, 0, 0, 1, 6);
        grid.Add(labelVideoResolution, 1, 0);
        grid.Add(numericUpDownWidth, 1, 1);
        grid.Add(labelX, 1, 2);
        grid.Add(numericUpDownHeight, 1, 3);
        grid.Add(buttonBrowseResolution, 1, 4);
        grid.Add(buttonFromCurrentVideo, 1, 5);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeOptionsView(AssaPropertiesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var label = UiUtil.MakeLabel("Options").WithBold().WithMarginBottom(10);

        var labelWrapStyle = UiUtil.MakeLabel("Wrap Style:");
        var comboBoxWrapStyle = UiUtil.MakeComboBox(vm.WrapStyles, vm, nameof(vm.SelectedWrapStyle));

        var labelBorderAndShadowScaling = UiUtil.MakeLabel("Border and Shadow Scaling:");
        var comboBoxBorderAndShadowScaling = UiUtil.MakeComboBox(vm.BorderAndShadowScalingStyles, vm, nameof(vm.SelectedBorderAndShadowScalingStyle));

        grid.Add(label, 0, 0, 1, 2);
        grid.Add(labelWrapStyle, 1, 0);
        grid.Add(comboBoxWrapStyle, 1, 1);
        grid.Add(labelBorderAndShadowScaling, 2, 0);
        grid.Add(comboBoxBorderAndShadowScaling, 2, 1);

        return UiUtil.MakeBorderForControl(grid);
    }
}
