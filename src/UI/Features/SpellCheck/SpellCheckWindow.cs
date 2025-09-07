using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public class SpellCheckWindow : Window
{
    private readonly SpellCheckViewModel _vm;

    public SpellCheckWindow(SpellCheckViewModel vm)
    {
        UiUtil.InitializeWindow(this);
        Title = Se.Language.SpellCheck.SpellCheck;
        SizeToContent = SizeToContent.Height;
        Width = 700;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelLine = new Label
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            [!Label.ContentProperty] = new Binding(nameof(SpellCheckViewModel.LineText), BindingMode.OneWay)
        };

        var buttonEditWholeText = UiUtil.MakeButton(vm.EditWholeTextCommand, IconNames.Pencil);
        buttonEditWholeText.HorizontalAlignment = HorizontalAlignment.Right;
        buttonEditWholeText.VerticalAlignment = VerticalAlignment.Top;
        buttonEditWholeText.Margin = new Thickness(0, 3, 0, 5);

        var panelWholeText = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        vm.PanelWholeText = panelWholeText;
        var scrollViewerWholeText = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = panelWholeText,
            Height = 85,
        };

        var boderWholeText = UiUtil.MakeBorderForControl(scrollViewerWholeText);

        var panelButtons = MakeWordNotFound(vm);


        var labelDictionary = new Label
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            Content = "Dictionary",
        };
        var panelSuggestions = MakeSuggestions(vm);

        var buttonDone = UiUtil.MakeButtonDone(vm.OkCommand).WithLeftAlignment();
        var panelButtonsOk = UiUtil.MakeButtonBar(buttonDone);

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
                new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 20,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };


        grid.Add(labelLine, 0, 0);
        grid.Add(boderWholeText, 1, 0);
        grid.Add(panelButtons, 2, 0);
        grid.Add(buttonEditWholeText, 2, 0);
        grid.Add(panelButtonsOk, 3, 1);

        grid.Add(labelDictionary, 0, 1);
        grid.Add(panelSuggestions, 1, 1, 2);

        Content = grid;

        Activated += delegate { buttonDone.Focus(); }; // hack to make OnKeyDown work
    }

    private static Grid MakeWordNotFound(SpellCheckViewModel vm)
    {
        var labelWordNotFound = new Label
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            Content = "Word not found",
            Margin = new Thickness(0, 13, 0, 0),
        };

        var textBoxWord = new TextBox
        {
            [!TextBox.TextProperty] = new Binding(nameof(SpellCheckViewModel.CurrentWord), BindingMode.TwoWay),
            VerticalAlignment = VerticalAlignment.Center,
            Width = double.NaN,
        };
        vm.TextBoxWordNotFound = textBoxWord;

        var buttonChange = new Button
        {
            Content = "Change",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.ChangeWordCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
        };

        var buttonChangeAll = new Button
        {
            Content = "Change all",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.ChangeWordAllCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
        };

        var buttonSkipOne = new Button
        {
            Content = "Skip one",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SkipWordCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
        };

        var buttonSkipAll = new Button
        {
            Content = "Skip all",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SkipWordAllCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
        };

        var buttonAddToNames = new Button
        {
            Content = "Add to names",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.AddToNamesListCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
        };

        var buttonAddToDictionary = new Button
        {
            Content = "Add to user dictionary",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.AddToUserDictionaryCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
        };

        var buttonGoogleSearch = new Button
        {
            Content = "Google it",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.GoogleItCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 0),
            ColumnSpacing = 5,
            RowSpacing = 0,
        };

        grid.Add(labelWordNotFound, 0, 0, 1, 2);
        grid.Add(textBoxWord, 1, 0, 1, 2);
        grid.Add(buttonChange, 2, 0);
        grid.Add(buttonChangeAll, 2, 1);
        grid.Add(buttonSkipOne, 3, 0);
        grid.Add(buttonSkipAll, 3, 1);
        grid.Add(buttonAddToNames, 4, 0, 1, 2);
        grid.Add(buttonAddToDictionary, 5, 0, 1, 2);
        grid.Add(buttonGoogleSearch, 7, 0, 1, 2);

        return grid;
    }

    private static Grid MakeSuggestions(SpellCheckViewModel vm)
    {
        var comboBoxDictionary = new ComboBox
        {
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(SpellCheckViewModel.Dictionaries), BindingMode.OneWay),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(SpellCheckViewModel.SelectedDictionary), BindingMode.TwoWay),
            VerticalAlignment = VerticalAlignment.Center,
            Width = 200,
        };

        var buttonDictionaryBrowse = new Button
        {
            Content = "...",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.BrowseDictionaryCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var panelDictionary = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
            Children =
            {
                comboBoxDictionary,
                buttonDictionaryBrowse,
            }
        };

        var labelSuggestions = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Content = "Suggestions",
            Margin = new Thickness(0, 10, 0, 0),
        };

        var listBoxSuggestions = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(SpellCheckViewModel.Suggestions), BindingMode.OneWay),
            [!ListBox.SelectedItemProperty] = new Binding(nameof(SpellCheckViewModel.SelectedSuggestion), BindingMode.TwoWay),
            VerticalAlignment = VerticalAlignment.Top,
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = new SolidColorBrush(Colors.Transparent),
        };
        listBoxSuggestions.DoubleTapped += vm.ListBoxSuggestionsDoubleTapped;

        var scrollViewSuggestions = new ScrollViewer
        {
            Content = listBoxSuggestions,
            Height = 243,
        };

        var borderSuggestions = UiUtil.MakeBorderForControl(scrollViewSuggestions);

        var buttonUseOnce = new Button
        {
            Content = "Use once",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SuggestionUseOnceCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonUseAlways = new Button
        {
            Content = "Use always",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SuggestionUseAlwaysCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 10),
            Spacing = 5,
            Children =
            {
                buttonUseOnce,
                buttonUseAlways,
            }
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
        };

        grid.Add(panelDictionary, 0, 0);
        grid.Add(labelSuggestions, 1, 0);
        grid.Add(borderSuggestions, 2, 0);
        grid.Add(panelButtons, 3, 0);

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
