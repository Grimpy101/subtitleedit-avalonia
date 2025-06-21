using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;

public partial class ChangeFrameRateViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<double> _fromFrameRates;
    [ObservableProperty] private double _selectedFromFrameRate;
    
    [ObservableProperty] private ObservableCollection<double> _toFrameRates;
    [ObservableProperty] private double _selectedToFrameRate;
    
    public Window? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public ChangeFrameRateViewModel()
    {
        FromFrameRates = new ObservableCollection<double> { 23.976, 24, 25, 29.97, 30, 50, 59.94, 60 };
        ToFrameRates = new ObservableCollection<double>(FromFrameRates);
        
        SelectedFromFrameRate = FromFrameRates[0];
        SelectedToFrameRate = ToFrameRates[0];
    }
    
    [RelayCommand]                   
    private void SwitchFrameRates() 
    {
        var temp = SelectedFromFrameRate;
        SelectedFromFrameRate = SelectedToFrameRate;
        SelectedToFrameRate = temp;
    }

    [RelayCommand]
    private void BrowseFromFrameRate()
    {
    }

    [RelayCommand]
    private void BrowseToFrameRate()
    {
    }

    [RelayCommand]                   
    private void Ok() 
    {
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void Cancel() 
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}