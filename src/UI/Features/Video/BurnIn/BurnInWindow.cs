using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class BurnInWindow : Window
{
    private readonly BurnInViewModel _vm;

    public BurnInWindow(BurnInViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.Video.BurnIn.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var subtitleSettingsView = MakeSubtitlesView(vm);
        var videoSettingsView = MakeVideoSettingsView(vm);
        var cutView = MakeCutView(vm);
        var previewView = MakePreviewView(vm);
        var audioSettingsView = MakeAudioSettingsView(vm);
        var batchView = MakeBatchView(vm);
        var targetFileSizeView = MakeTargetFileSizeView(vm);
        var videoInfoView = MakeVideoInfoView(vm);
        var progressView = MakeProgressView(vm);

        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand)
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonBatchMode = UiUtil.MakeButton(Se.Language.General.BatchMode, vm.BatchModeCommand)
            .WithBindIsVisible(nameof(vm.IsBatchMode), new InverseBooleanConverter())
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonHelp = UiUtil.MakeButton(Se.Language.General.Help, vm.HelpCommand);
        var buttonSingleMode = UiUtil.MakeButton(Se.Language.General.SingleMode, vm.SingleModeCommand)
            .WithBindIsVisible(nameof(vm.IsSingleModeVisible))
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonGenerate,
            buttonHelp,
            buttonBatchMode,
            buttonSingleMode,
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // target file size + video info
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress bar
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // subtitle/video settings
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // cut/preview/audio settings
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // batch mode
            },
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(subtitleSettingsView, 0, 0, 2, 1);
        grid.Add(videoSettingsView, 2, 0);
        grid.Add(cutView, 0, 1);
        grid.Add(previewView, 1, 1);
        grid.Add(audioSettingsView, 2, 1);
        grid.Add(batchView, 0, 3, 3, 1);
        grid.Add(targetFileSizeView, 4, 0);
        grid.Add(videoInfoView, 4, 1);
        grid.Add(progressView, 5, 0, 1, 3);
        grid.Add(buttonPanel, 6, 0, 1, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private static Border MakeSubtitlesView(BurnInViewModel vm)
    {
        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboBoxFontName = UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName))
            .WithMinWidth(200);
        comboBoxFontName.SelectionChanged += vm.ComboBoxChanged;

        var labelFontSizeFactor = UiUtil.MakeLabel(Se.Language.Video.BurnIn.FontSizeFactor);
        var numericUpDownFontSizeFactor = UiUtil.MakeNumericUpDownTwoDecimals(0.1m, 1.0m, 150, vm, nameof(vm.FontFactor));
        numericUpDownFontSizeFactor.ValueChanged += vm.NumericUpDownChanged;
        var labelFontSizeFactorInfo = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.FontFactorText));
        var panelFontSizeFactor = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                numericUpDownFontSizeFactor,
                labelFontSizeFactorInfo
            }
        };

        var checkBoxUseBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.FontIsBold));
        checkBoxUseBold.PropertyChanged += vm.CheckBoxChanged;

        var labelTextColor = UiUtil.MakeLabel(Se.Language.General.TextColor);
        var colorPickerTextColor = UiUtil.MakeColorPicker(vm, nameof(vm.FontTextColor));
        colorPickerTextColor.ColorChanged += vm.ColorChanged;

        var labelOutline = UiUtil.MakeLabel(string.Empty)
            .WithBindText(vm, nameof(vm.FontOutlineText));
        var textBoxBoxWidth = UiUtil.MakeTextBox(50, vm, nameof(vm.SelectedFontOutline));
        textBoxBoxWidth.TextChanged += vm.TextBoxChanged;
        var colorPickerBoxColor = UiUtil.MakeColorPicker(vm, nameof(vm.FontOutlineColor));
        colorPickerBoxColor.ColorChanged += vm.ColorChanged;
        var panelBox = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                colorPickerBoxColor,
                UiUtil.MakeLabel(Se.Language.General.Width).WithMarginLeft(5),
                textBoxBoxWidth,
            }
        };

        var labelShadow = UiUtil.MakeLabel(Se.Language.General.Shadow)
            .WithBindText(vm, nameof(vm.FontShadowText));
        var textBoxShadowWidth = UiUtil.MakeTextBox(50, vm, nameof(vm.SelectedFontShadowWidth));
        textBoxShadowWidth.TextChanged += vm.TextBoxChanged;
        var colorPickerShadowColor = UiUtil.MakeColorPicker(vm, nameof(vm.FontShadowColor));
        colorPickerShadowColor.ColorChanged += vm.ColorChanged;
        var panelShadow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                colorPickerShadowColor,
                UiUtil.MakeLabel(Se.Language.General.Width).WithMarginLeft(5),
                textBoxShadowWidth,
            }
        };

        var labelBoxType = UiUtil.MakeLabel(Se.Language.Video.BurnIn.BoxType);
        var comboBoxBoxType = UiUtil.MakeComboBox(vm.FontBoxTypes, vm, nameof(vm.SelectedFontBoxType));
        comboBoxBoxType.SelectionChanged += vm.BoxTypeChanged;

        var labelAlignment = UiUtil.MakeLabel(Se.Language.General.Alignment);
        var comboBoxAlignment = UiUtil.MakeComboBox(vm.FontAlignments, vm, nameof(vm.SelectedFontAlignment));
        comboBoxAlignment.SelectionChanged += vm.ComboBoxChanged;

        var labelMargin = UiUtil.MakeLabel(Se.Language.General.Margin);
        var labelMarginHorizontal = UiUtil.MakeLabel(Se.Language.General.Horizontal);
        var textBoxMarginHorizontal = UiUtil.MakeTextBox(50, vm, nameof(vm.FontMarginHorizontal));
        textBoxMarginHorizontal.TextChanged += vm.TextBoxChanged;
        var labelMarginVertical = UiUtil.MakeLabel(Se.Language.General.Vertical).WithMarginLeft(5);
        var textBoxMarginVertical = UiUtil.MakeTextBox(50, vm, nameof(vm.FontMarginVertical));
        textBoxMarginVertical.TextChanged += vm.TextBoxChanged;
        var panelMargin = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelMarginHorizontal,
                textBoxMarginHorizontal,
                labelMarginVertical,
                textBoxMarginVertical
            }
        };

        var checkBoxFixRightToLeft = UiUtil.MakeCheckBox(Se.Language.Video.BurnIn.FixRightToLeft, vm, nameof(vm.FontFixRtl));
        checkBoxFixRightToLeft.PropertyChanged += vm.CheckBoxChanged;

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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelFontName, 0, 0);
        grid.Add(comboBoxFontName, 0, 1);

        grid.Add(labelFontSizeFactor, 1, 0);
        grid.Add(panelFontSizeFactor, 1, 1);

        grid.Add(checkBoxUseBold, 2, 1);

        grid.Add(labelBoxType, 3, 0);
        grid.Add(comboBoxBoxType, 3, 1);

        grid.Add(labelTextColor, 4, 0);
        grid.Add(colorPickerTextColor, 4, 1);

        grid.Add(labelOutline, 5, 0);
        grid.Add(panelBox, 5, 1);

        grid.Add(labelShadow, 6, 0);
        grid.Add(panelShadow, 6, 1);

        grid.Add(labelAlignment, 7, 0);
        grid.Add(comboBoxAlignment, 7, 1);

        grid.Add(labelMargin, 8, 0);
        grid.Add(panelMargin, 8, 1);

        grid.Add(checkBoxFixRightToLeft, 9, 1);

        var panel = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            Background = Brushes.Black,
            Opacity = 0.8,
            Children =
            {
                new Label
                {
                    Content = "Current ASSA style will be used" + Environment.NewLine +
                    Environment.NewLine +
                    "Change subtitle format if"+ Environment.NewLine + 
                    "you want to set styles here",
                    FontWeight = FontWeight.Bold,
                    FontSize = 22,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                },
            }
        }.WithBindVisible(vm, nameof(vm.ShowAssaOnlyBox));
        grid.Add(panel, 0, 0, 10, 2);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakeVideoSettingsView(BurnInViewModel vm)
    {
        var labelResolution = UiUtil.MakeLabel(Se.Language.General.Resolution);
        var textBoxWidth = UiUtil.MakeTextBox(100, vm, nameof(vm.VideoWidth));
        var labelX = UiUtil.MakeLabel("x");
        var textBoxHeight = UiUtil.MakeTextBox(100, vm, nameof(vm.VideoHeight));
        var buttonResolution = UiUtil.MakeButtonBrowse(vm.BrowseResolutionCommand);
        var panelResolution = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                textBoxWidth,
                labelX,
                textBoxHeight,
                buttonResolution,
            }
        }.WithBindVisible(vm, nameof(vm.UseSourceResolution), new InverseBooleanConverter());

        var labelSourceResolution = UiUtil.MakeLabel("Use source resolution").WithBindVisible(vm, nameof(vm.UseSourceResolution));
        var buttonResolutionSource = UiUtil.MakeButtonBrowse(vm.BrowseResolutionCommand);
        var panelResolutionSource = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                labelSourceResolution,
                buttonResolutionSource,
            }
        }.WithBindVisible(vm, nameof(vm.UseSourceResolution));

        var labelEncoding = UiUtil.MakeLabel(Se.Language.General.Encoding);
        var comboBoxEncoding = UiUtil.MakeComboBox(vm.VideoEncodings, vm, nameof(vm.SelectedVideoEncoding));
        comboBoxEncoding.SelectionChanged += vm.VideoEncodingChanged;

        var labelPreset = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoPresetText));
        var comboBoxPreset = UiUtil.MakeComboBox(vm.VideoPresets, vm, nameof(vm.SelectedVideoPreset));

        var labelCrf = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoCrfText));
        var comboBoxCrf = UiUtil.MakeComboBox(vm.VideoCrf, vm, nameof(vm.SelectedVideoCrf));
        var labelCrfHint = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoCrfHint)).WithMarginLeft(5);
        labelCrfHint.FontSize = 10;
        labelCrfHint.Opacity = 0.7;
        var panelCrf = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                comboBoxCrf,
                labelCrfHint
            }
        };

        var labelPixelFormat = UiUtil.MakeLabel(Se.Language.Video.BurnIn.PixelFormat);
        var comboBoxPixelFormat = UiUtil.MakeComboBox(vm.VideoPixelFormats, vm, nameof(vm.SelectedVideoPixelFormat));

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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelResolution, 0, 0);
        grid.Add(panelResolution, 0, 1);
        grid.Add(panelResolutionSource, 0, 1);

        grid.Add(labelEncoding, 1, 0);
        grid.Add(comboBoxEncoding, 1, 1);

        grid.Add(labelPreset, 2, 0);
        grid.Add(comboBoxPreset, 2, 1);

        grid.Add(labelCrf, 3, 0);
        grid.Add(panelCrf, 3, 1);

        grid.Add(labelPixelFormat, 4, 0);
        grid.Add(comboBoxPixelFormat, 4, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakeCutView(BurnInViewModel vm)
    {
        var checkBoxCut = UiUtil.MakeCheckBox(Se.Language.Video.BurnIn.Cut, vm, nameof(vm.IsCutActive));

        var buttonCutFrom = UiUtil.MakeButtonBrowse(vm.BrowseCutFromCommand).WithMarginTop(10).WithRightAlignment();
        var labelFromTime = UiUtil.MakeLabel(Se.Language.Video.BurnIn.FromTime);
        var timeUpDownFrom = new TimeCodeUpDown
        {
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.CutFrom)),
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var buttonCutTo = UiUtil.MakeButtonBrowse(vm.BrowseCutToCommand).WithMarginTop(10).WithRightAlignment();
        var labelToTime = UiUtil.MakeLabel(Se.Language.Video.BurnIn.ToTime);
        var timeUpDownTo = new TimeCodeUpDown
        {
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.CutTo)),
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxCut, 0, 0);

        grid.Add(buttonCutFrom, 1, 1);
        grid.Add(labelFromTime, 2, 0);
        grid.Add(timeUpDownFrom, 2, 1);

        grid.Add(buttonCutTo, 3, 1);
        grid.Add(labelToTime, 4, 0);
        grid.Add(timeUpDownTo, 4, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakePreviewView(BurnInViewModel vm)
    {

        var labelPreview = UiUtil.MakeLabel(Se.Language.General.Preview);

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.None, // Prevents stretching of the image
        };

        var scrollViewer = new ScrollViewer
        {
            Content = image,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = 300,
            Height = double.NaN,
        };

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
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelPreview, 0, 0);
        grid.Add(scrollViewer, 1, 0);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakeAudioSettingsView(BurnInViewModel vm)
    {
        var labelAudioEncoding = UiUtil.MakeLabel(Se.Language.Video.BurnIn.AudioEncoding);
        var comboBoxAudioEncoding = UiUtil.MakeComboBox(vm.AudioEncodings, vm, nameof(vm.SelectedAudioEncoding));

        var checkBoxStereo = UiUtil.MakeCheckBox(Se.Language.Video.BurnIn.Stereo, vm, nameof(vm.AudioIsStereo));

        var labelSampleRate = UiUtil.MakeLabel(Se.Language.Video.BurnIn.SampleRate);
        var comboBoxSampleRate = UiUtil.MakeComboBox(vm.AudioSampleRates, vm, nameof(vm.SelectedAudioSampleRate));

        var labelBitRate = UiUtil.MakeLabel(Se.Language.Video.BurnIn.BitRate);
        var comboBoxBitRate = UiUtil.MakeComboBox(vm.AudioBitRates, vm, nameof(vm.SelectedAudioBitRate));

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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelAudioEncoding, 0, 0);
        grid.Add(comboBoxAudioEncoding, 0, 1);

        grid.Add(checkBoxStereo, 1, 1);

        grid.Add(labelSampleRate, 2, 0);
        grid.Add(comboBoxSampleRate, 2, 1);

        grid.Add(labelBitRate, 3, 0);
        grid.Add(comboBoxBitRate, 3, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakeBatchView(BurnInViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.JobItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BurnInJobItem.InputVideoFileNameShort)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Size,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BurnInJobItem.Resolution)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.SubtitleFile,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BurnInJobItem.SubtitleFileNameShort)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Status,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(BurnInJobItem.Status)),
                    IsReadOnly = true,
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedJobItem)) { Source = vm });

        var buttonAdd = UiUtil.MakeButton(Se.Language.General.AddDotDotDot, vm.AddCommand);
        var buttonRemove = UiUtil.MakeButton(Se.Language.General.Remove, vm.RemoveCommand);
        var buttonClear = UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearCommand);
        var buttonPickSubtitle = UiUtil.MakeButton(Se.Language.General.PickSubtitleFile, vm.PickSubtitleCommand);

        var panelFileControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonAdd,
                buttonRemove,
                buttonClear,
                UiUtil.MakeSeparatorForHorizontal(),
                buttonPickSubtitle,
            }
        };

        var buttonOutputProperties = UiUtil.MakeButton(Se.Language.Video.BurnIn.OutputProperties, vm.OutputPropertiesCommand);
        var labelOutputPropertiesFolder = UiUtil.MakeLink(string.Empty, vm.OpenOutputFolderCommand)
            .WithBindText(vm, nameof(vm.OutputFolder))
            .WithBindVisible(vm, nameof(vm.UseOutputFolderVisible));
        var labelOutputPropertiesUseSourceFolder = UiUtil.MakeLabel(Se.Language.Video.BurnIn.UseSourceFolder)
            .WithBindVisible(vm, nameof(vm.UseSourceFolderVisible));

        var panelFileControls2 = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonOutputProperties,
                labelOutputPropertiesFolder,
                labelOutputPropertiesUseSourceFolder,
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(dataGrid, 0, 0);
        grid.Add(panelFileControls, 1, 0);
        grid.Add(panelFileControls2, 2, 0);

        return UiUtil.MakeBorderForControl(grid)
            .WithBindIsVisible(nameof(vm.IsBatchMode))
            .WithMarginBottom(5);
    }

    private static Border MakeTargetFileSizeView(BurnInViewModel vm)
    {
        var checkBoxUseTargetFileSize = UiUtil.MakeCheckBox(Se.Language.Video.BurnIn.TargetFileSize, vm, nameof(vm.UseTargetFileSize));
        checkBoxUseTargetFileSize.PropertyChanged += vm.CheckBoxTargetFileChanged;

        var labelTargetFileSize = UiUtil.MakeLabel(Se.Language.Video.BurnIn.FileSizeMb);
        var numericUpDownTargetFileSize = UiUtil.MakeNumericUpDownInt(1, 1000_000_000, 150, vm, nameof(vm.TargetFileSize));
        numericUpDownTargetFileSize.ValueChanged += vm.NumericUpDownTargetFileSizeChanged;
        var labelVideoBitRate = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.TargetVideoBitRateInfo));
        labelVideoBitRate.FontSize = 10;
        labelVideoBitRate.Opacity = 0.7;
        var panelTargetFileSize = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                numericUpDownTargetFileSize,
                labelVideoBitRate
            }
        };

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
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxUseTargetFileSize, 0, 0, 1, 2);
        grid.Add(labelTargetFileSize, 1, 0);
        grid.Add(panelTargetFileSize, 1, 1);

        return UiUtil.MakeBorderForControl(grid)
            .WithBindIsVisible(nameof(vm.IsBatchMode), new InverseBooleanConverter())
            .WithMarginRight(5);
    }

    private static Border MakeVideoInfoView(BurnInViewModel vm)
    {
        var labelVideoFile = UiUtil.MakeLabel(Se.Language.General.VideoFile);
        var labelVideoFileName = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoFileName));

        var labelVideoSize = UiUtil.MakeLabel(Se.Language.Video.BurnIn.VideoFileSize);
        var labelVideoSizeValue = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoFileSize));

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
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelVideoFile, 0, 0);
        grid.Add(labelVideoFileName, 0, 1);

        grid.Add(labelVideoSize, 1, 0);
        grid.Add(labelVideoSizeValue, 1, 1);

        return UiUtil.MakeBorderForControl(grid)
            .WithBindIsVisible(nameof(vm.IsBatchMode), new InverseBooleanConverter())
            .WithMarginRight(5);
    }

    private static Grid MakeProgressView(BurnInViewModel vm)
    {
        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressSlider.Bind(Slider.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var statusText = new TextBlock
        {
            Margin = new Thickness(5, 20, 0, 0),
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        statusText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(progressSlider, 0, 0);
        grid.Add(statusText, 0, 0);

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
