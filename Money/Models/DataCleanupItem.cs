using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiIcons.Material;

namespace Money.Models;

public sealed class DataCleanupItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public MaterialIcons Icon { get; init; }
    public long Count { get; set; }
    public string CountText => $"{Count:N0} registro(s)";

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
