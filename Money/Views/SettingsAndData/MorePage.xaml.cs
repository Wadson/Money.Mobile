using Money.Services;

namespace Money.Views.SettingsAndData;

public partial class MorePage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly AuthService _auth;
    private readonly BackupService _backup;

    public MorePage(DatabaseService database, AuthService auth, BackupService backup)
    {
        InitializeComponent();
        _database = database;
        _auth = auth;
        _backup = backup;
    }

    private async void OnBudgetRuleTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new BudgetRuleAnalysisPage(_database));
    private async void OnCardsTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new CreditCardAnalysisPage(_database));
    private async void OnManageCardsTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new CardManagementPage(_database));
    private async void OnReportsTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new ReportPage(_database));
    private async void OnMonthlyTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new MonthlyOverviewPage(_database));
    private async void OnCategoriesTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await Navigation.PushModalAsync(new CategoryManagementPage(_database));
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Categorias", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }
    private async void OnSubcategoriesTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new SubcategoryManagementPage(_database));
    private async void OnSuppliersTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await _database.InitializeAsync();
            await Navigation.PushModalAsync(new SupplierManagementPage(_database));
        }
        catch (Exception ex)
        {
            await ThemedDialog.ShowAsync(this, "Fornecedores", SqliteErrorMessage.ToFriendly(ex), "Fechar");
        }
    }
    private async void OnProfileTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new ProfilePage(_auth));
    private async void OnUsersTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new UserManagementPage(_auth));
    private async void OnSettingsTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new SettingsPage(_database));
    private async void OnPaymentReversalTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new PaymentReversalPage(_database));
    private async void OnBackupTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(new BackupPage(_backup));
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();
}
