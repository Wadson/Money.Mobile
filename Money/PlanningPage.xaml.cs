using System.Globalization;
using MauiIcons.Core;
using MauiIcons.Material;
using Money.Models;
using Money.Services;

namespace Money;

public partial class PlanningPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("pt-BR");
    private List<CategoryItem> _categories = [];

    public PlanningPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
        TagColorEntry.Text = BlingPalette.PrimaryHex;
        GoalPriorityPicker.ItemsSource = new[] { "Baixa", "Média", "Alta" };
        GoalPriorityPicker.SelectedIndex = 1;
        GoalStartPicker.Date = DateTime.Today;
        GoalDuePicker.Date = DateTime.Today.AddMonths(1);
        GoalDuePicker.MinimumDate = DateTime.Today;
        ReminderDatePicker.Date = DateTime.Today;
        ReminderDatePicker.MinimumDate = DateTime.Today;
        TagIconPicker.ItemsSource = new[] { "Etiqueta", "Favorito", "Trabalho", "Casa", "Viagem" };
        TagIconPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        try
        {
            ErrorLabel.IsVisible = false;
            _categories = await _database.GetCategoriesAsync("despesa");
            GoalCategoryPicker.ItemsSource = new[] { "Sem categoria" }
                .Concat(_categories.Select(x => x.FullPath ?? x.Name)).ToList();
            if (GoalCategoryPicker.SelectedIndex < 0) GoalCategoryPicker.SelectedIndex = 0;
            RenderGoals(await _database.GetMetasAsync());
            RenderReminders(await _database.GetLembretesAsync());
            RenderTags(await _database.GetTagsAsync());
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void RenderGoals(IEnumerable<MetaItem> goals)
    {
        GoalsContainer.Children.Clear();
        foreach (var goal in goals)
        {
            var progress = goal.TargetValue <= 0 ? 0 : Math.Clamp((double)(goal.CurrentValue / goal.TargetValue), 0, 1);
            var layout = new VerticalStackLayout { Spacing = 3 };
            layout.Children.Add(new Label { Text = goal.Name, FontAttributes = FontAttributes.Bold, TextColor = ThemeColor.Get("BlingPrimary") });
            layout.Children.Add(new Label { Text = $"{goal.CurrentValue.ToString("C2", _culture)} de {goal.TargetValue.ToString("C2", _culture)} · {goal.Priority} · até {goal.TargetDate:dd/MM/yyyy}", FontSize = 11, TextColor = ThemeColor.Get("BlingText") });
            layout.Children.Add(new ProgressBar { Progress = progress, ProgressColor = ThemeColor.Get("BlingPrimary") });
            if (goal.Status == "em_andamento")
            {
                var contribute = new Button { Text = "Registrar aporte", FontSize = 11, TextColor = ThemeColor.Get("BlingTextLight"), BackgroundColor = ThemeColor.Get("BlingPrimary"), CommandParameter = goal.Id };
                contribute.Clicked += OnContributeClicked;
                layout.Children.Add(contribute);
            }
            var delete = new Button { Text = "Excluir", FontSize = 11, TextColor = ThemeColor.Get("BlingTextLight"), BackgroundColor = ThemeColor.Get("BlingPrimary"), CommandParameter = goal.Id };
            delete.Clicked += OnDeleteGoalClicked;
            layout.Children.Add(delete);
            GoalsContainer.Children.Add(new Border { Content = layout, BackgroundColor = ThemeColor.Get("BlingCard"), Padding = 10, StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 } });
        }
    }

    private async void OnSelectGoalCategoryClicked(object? sender, EventArgs e)
    {
        var options = new List<SelectionOption>
        {
            new() { Index = 0, Label = "Sem categoria", ImageSource = MaterialIcons.Category,
                Background = ThemeColor.Get("BlingCard"), Foreground = ThemeColor.Get("BlingPrimary"),
                IsSelected = GoalCategoryPicker.SelectedIndex == 0 }
        };
        options.AddRange(_categories.Select((category, index) =>
            CategoryVisualResolver.Option(category, index + 1, GoalCategoryPicker.SelectedIndex == index + 1)));
        var page = new OptionSelectionPage("Selecione a categoria", options);
        page.Selected += (_, option) => { GoalCategoryPicker.SelectedIndex = option.Index; GoalCategoryButton.Text = option.Label; };
        await Navigation.PushModalAsync(page);
    }

    private void RenderReminders(IEnumerable<LembreteItem> reminders)
    {
        RemindersContainer.Children.Clear();
        foreach (var reminder in reminders)
        {
            var row = new Grid { ColumnDefinitions = [new(GridLength.Star), new(GridLength.Auto)], Padding = 8, BackgroundColor = ThemeColor.Get("BlingCard") };
            row.Add(new Label { Text = $"{reminder.Title}\n{reminder.ReminderDate:dd/MM/yyyy} · {reminder.Status}", TextColor = ThemeColor.Get("BlingText"), FontSize = 12 });
            var actions = new VerticalStackLayout { Spacing = 2 };
            if (reminder.Status == "pendente")
            {
                var done = new Button { Text = "Executado", FontSize = 9, HeightRequest = 32, TextColor = ThemeColor.Get("BlingTextLight"), BackgroundColor = ThemeColor.Get("BlingPrimary"), CommandParameter = reminder.Id };
                done.Clicked += OnCompleteReminderClicked;
                actions.Children.Add(done);
            }
            var delete = new Button { Text = "Excluir", FontSize = 9, HeightRequest = 32, TextColor = ThemeColor.Get("BlingTextLight"), BackgroundColor = ThemeColor.Get("BlingPrimary"), CommandParameter = reminder.Id };
            delete.Clicked += OnDeleteReminderClicked;
            actions.Children.Add(delete);
            row.Add(actions, 1);
            RemindersContainer.Children.Add(row);
        }
    }

    private void RenderTags(IEnumerable<TagItem> tags)
    {
        TagsContainer.Children.Clear();
        foreach (var tag in tags)
        {
            var button = new Button { Text = tag.Name, ImageSource = MaterialIcons.Close.ToImageSource(ThemeColor.Get("BlingTextLight"), 16), FontSize = 11, TextColor = ThemeColor.Get("BlingTextLight"), BackgroundColor = ParseColor(tag.Color), CornerRadius = 14, Margin = 3, CommandParameter = tag.Id };
            button.Clicked += OnDeleteTagClicked;
            TagsContainer.Children.Add(button);
        }
    }

    private async void OnSaveGoalClicked(object? sender, EventArgs e)
    {
        if (!TryMoney(GoalTargetEntry.Text, out var target) || target <= 0) { ShowError("Informe um valor objetivo maior que zero."); return; }
        if (!TryMoney(GoalCurrentEntry.Text, out var current)) current = 0;
        var category = GoalCategoryPicker.SelectedIndex > 0 ? _categories[GoalCategoryPicker.SelectedIndex - 1].FullPath ?? _categories[GoalCategoryPicker.SelectedIndex - 1].Name : null;
        try
        {
            await _database.SaveMetaAsync(null, GoalNameEntry.Text ?? "", target, current,
                GoalStartPicker.Date ?? DateTime.Today, GoalDuePicker.Date ?? DateTime.Today,
                category, new[] { "baixa", "media", "alta" }[Math.Max(0, GoalPriorityPicker.SelectedIndex)],
                "em_andamento", GoalNotesEditor.Text);
            GoalNameEntry.Text = GoalTargetEntry.Text = GoalCurrentEntry.Text = GoalNotesEditor.Text = "";
            await ReloadAsync();
            await ThemedDialog.ShowAsync(this, "Meta salva", "Meta cadastrada com sucesso!");
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void OnSaveReminderClicked(object? sender, EventArgs e)
    {
        long? transaction = long.TryParse(ReminderTransactionEntry.Text, out var id) ? id : null;
        try
        {
            await _database.SaveLembreteAsync(null, transaction, ReminderTitleEntry.Text ?? "",
                ReminderDescriptionEditor.Text, ReminderDatePicker.Date ?? DateTime.Today);
            ReminderTitleEntry.Text = ReminderDescriptionEditor.Text = ReminderTransactionEntry.Text = "";
            await ReloadAsync();
            await ThemedDialog.ShowAsync(this, "Lembrete salvo", "Lembrete cadastrado com sucesso!");
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void OnSaveTagClicked(object? sender, EventArgs e)
    {
        try
        {
            var icons = new[] { "tag", "favorite", "work", "home", "travel" };
            await _database.SaveTagAsync(null, TagNameEntry.Text ?? "", TagColorEntry.Text ?? BlingPalette.PrimaryHex,
                icons[Math.Max(0, TagIconPicker.SelectedIndex)]);
            TagNameEntry.Text = ""; await ReloadAsync();
            await ThemedDialog.ShowAsync(this, "Tag salva", "Tag cadastrada com sucesso!");
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void OnDeleteGoalClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id } || !await ThemedDialog.ConfirmDeleteAsync(this, "Excluir meta", "Deseja excluir esta meta?")) return;
        try { await _database.DeleteMetaAsync(id); await ReloadAsync(); await ThemedDialog.ShowAsync(this, "Meta excluída", "Meta excluída com sucesso!"); } catch (Exception ex) { ShowError(ex); }
    }

    private async void OnContributeClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id }) return;
        var value = await DisplayPromptAsync("Registrar aporte", "Informe o valor do aporte:",
            "Adicionar", "Cancelar", keyboard: Keyboard.Numeric);
        if (value is null) return;
        if (!TryMoney(value, out var amount) || amount <= 0) { ShowError("Informe um aporte maior que zero."); return; }
        try { await _database.AddMetaContributionAsync(id, amount); await ReloadAsync(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void OnDeleteReminderClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id } || !await ThemedDialog.ConfirmDeleteAsync(this, "Excluir lembrete", "Deseja excluir este lembrete?")) return;
        try { await _database.DeleteLembreteAsync(id); await ReloadAsync(); await ThemedDialog.ShowAsync(this, "Lembrete excluído", "Lembrete excluído com sucesso!"); } catch (Exception ex) { ShowError(ex); }
    }

    private async void OnCompleteReminderClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id }) return;
        try { await _database.UpdateReminderStatusAsync(id, "executado"); await ReloadAsync(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void OnDeleteTagClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: long id } || !await ThemedDialog.ConfirmDeleteAsync(this, "Excluir tag", "Os vínculos desta tag também serão removidos. Continuar?")) return;
        try { await _database.DeleteTagAsync(id); await ReloadAsync(); await ThemedDialog.ShowAsync(this, "Tag excluída", "Tag excluída com sucesso!"); } catch (Exception ex) { ShowError(ex); }
    }

    private bool TryMoney(string? value, out decimal result) =>
        decimal.TryParse(value, NumberStyles.Currency, _culture, out result);
    private void OnTagColorClicked(object? sender, EventArgs e)
    {
        if (sender is Button button) TagColorEntry.Text = button.BackgroundColor.ToHex();
    }
    private static Color ParseColor(string value) => ThemeColor.Parse(value);
    private void ShowError(Exception ex) => ShowError(SqliteErrorMessage.ToFriendly(ex));
    private void ShowError(string message) { ErrorLabel.Text = message; ErrorLabel.IsVisible = true; }
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
