using Microsoft.Extensions.Logging;

using MauiIcons.Material;
using Money.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Money
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMaterialMauiIcons()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<BackupService>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<RecoveryPage>();
            builder.Services.AddTransient<TransactionFormPage>();
            builder.Services.AddTransient<CategoryManagementPage>();
            builder.Services.AddTransient<SupplierManagementPage>();
            builder.Services.AddTransient<CardManagementPage>();
            builder.Services.AddTransient<AccountsPayablePage>();
            builder.Services.AddTransient<IncomeListPage>();
            builder.Services.AddTransient<CreditCardAnalysisPage>();
            builder.Services.AddTransient<MonthlyOverviewPage>();
            builder.Services.AddTransient<MorePage>();
            builder.Services.AddTransient<BackupPage>();
            builder.Services.AddTransient<PaymentReversalPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<DataCleanupPage>();
            builder.Services.AddTransient<PayablesCsvImportPage>();
            builder.Services.AddTransient<ImportCardInvoicePage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
