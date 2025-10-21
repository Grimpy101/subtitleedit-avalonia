using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

public class ErrorListWindow : Window
{
    public ErrorListWindow(ErrorListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ListErrors;
        CanResize = true;
        Width = 600;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonGoTo = UiUtil.MakeButton(Se.Language.General.GoTo, vm.GoToCommand).WithBindIsEnabled(nameof(vm.HasBookmarks));
        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonGoTo, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeErrorsGridView(vm), 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work

        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Border MakeErrorsGridView(ErrorListViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            Margin = new Thickness(2),
            ItemsSource = vm.Subtitles, // Use ItemsSource instead of Items
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
            DataContext = vm.Subtitles,
        };

        dataGrid.DoubleTapped += vm.OnBookmarksGridDoubleTapped;

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // Columns
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ErrorListItem.Number)),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(ErrorListItem.Text)),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star) // star sizing to take all available space
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Error,
            Binding = new Binding(nameof(ErrorListItem.Error)),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star) // star sizing to take all available space
        });

        dataGrid.DataContext = vm.Subtitles;
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });
        dataGrid.SelectionChanged += vm.GridSelectionChanged;
        dataGrid.DoubleTapped += (s, e) => vm.GoToCommand.Execute(null);    
        dataGrid.KeyDown += (s, e) => vm.GridKeyDown(e);  

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
