using System.Collections.ObjectModel;
using MauiIcons.Material;

namespace Money.Views.Dialogs;

public sealed class SelectionOption : System.ComponentModel.INotifyPropertyChanged
{
    private bool _isSelected;

    public int Index { get; init; }
    public string Label { get; init; } = "";
    public string? Subtitle { get; init; }
    public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);
    public MaterialIcons Icon { get; init; } = MaterialIcons.Category;
    public MaterialIcons ImageSource { get; init; } = MaterialIcons.Category;
    public Color Background { get; init; } = Colors.Transparent;
    public Color Foreground { get; init; } = ThemeColor.Get("BlingText");
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this,
                new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}

public partial class OptionSelectionPage : ContentPage
{
    private readonly List<SelectionOption> _options;
    private readonly bool _requireConfirmation;
    private SelectionOption? _selectedOption;
    public event EventHandler<SelectionOption>? Selected;

    public OptionSelectionPage(string title, IEnumerable<SelectionOption> options,
        bool requireConfirmation = false, bool allowClear = false)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        _options = options.ToList();
        _requireConfirmation = requireConfirmation;
        _selectedOption = _options.FirstOrDefault(option => option.IsSelected);
        ConfirmButton.IsVisible = requireConfirmation;
        ConfirmButton.IsEnabled = _selectedOption is not null;
        ClearButton.IsVisible = allowClear;
        OptionsList.ItemsSource = new ObservableCollection<SelectionOption>(_options);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var term = e.NewTextValue?.Trim();
        var filtered = string.IsNullOrWhiteSpace(term)
            ? _options
            : _options.Where(x => x.Label.Contains(term, StringComparison.CurrentCultureIgnoreCase)).ToList();
        OptionsList.ItemsSource = new ObservableCollection<SelectionOption>(filtered);
    }

    private async void OnOptionTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not SelectionOption option) return;
        option.IsSelected = true;
        await CompleteSelectionAsync(option);
    }

    private async void OnSelectButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: SelectionOption option }) return;
        option.IsSelected = true;
        await CompleteSelectionAsync(option);
    }

    private async Task CompleteSelectionAsync(SelectionOption option)
    {
        foreach (var item in _options) item.IsSelected = ReferenceEquals(item, option);
        _selectedOption = option;
        ConfirmButton.IsEnabled = true;
        if (_requireConfirmation) return;
        Selected?.Invoke(this, option);
        await Navigation.PopModalAsync();
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (_selectedOption is null) return;
        Selected?.Invoke(this, _selectedOption);
        await Navigation.PopModalAsync();
    }

    private async void OnClearClicked(object? sender, EventArgs e)
    {
        Selected?.Invoke(this, new SelectionOption { Index = -1, Label = "Nenhuma seleção" });
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
