using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitFooter
{
    public static Grid Make(MainViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            RowDefinitions = new RowDefinitions("Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(5, 0, 5, 0),
        };
        
        vm.StatusTextLeftLabel = new TextBlock
        {
            Text = string.Empty,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
        };
        grid.Add(vm.StatusTextLeftLabel, 0);
        vm.StatusTextLeftLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusTextLeft)));

        var right = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 4),
            DataContext = vm,
        };
        right.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusTextRight)));

        var panelRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Icon
                {
                    Value = IconNames.LockClock,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.LockTimeCodes)),
                    FontSize = 20,
                    [ToolTip.TipProperty] = Se.Language.General.LockTimeCodes,
                },
                new Icon
                {
                    Value = IconNames.Filter,
                    [!Visual.IsVisibleProperty] = new Binding(nameof(vm.ShowLayerFilterIcon)),
                    FontSize = 20,
                    [ToolTip.TipProperty] = Se.Language.General.LayerFilterOn,
                },
                right,
            },
        };  

        grid.Add(panelRight, 0, 1);

        return grid;
    }
}