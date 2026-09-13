using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

using MauiIcons.Material;
using Money.Services;
using Money.Views.Auth;
using Money.Views.Dashboard;
using Money.Views.ImportsAndReports;
using Money.Views.Management;
using Money.Views.SettingsAndData;
using Money.Views.Transactions;

namespace Money
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
#if ANDROID
            static void AlignTextField(Android.Widget.TextView nativeView)
            {
                nativeView.Gravity = Android.Views.GravityFlags.Start | Android.Views.GravityFlags.CenterVertical;
                nativeView.TextAlignment = Android.Views.TextAlignment.ViewStart;
                var left = (int)(16 * DeviceDisplay.MainDisplayInfo.Density);
                nativeView.SetPadding(left, nativeView.PaddingTop, nativeView.PaddingRight, nativeView.PaddingBottom);
            }
            EntryHandler.Mapper.AppendToMapping("MoneyLeftAlignment", (handler, _) => AlignTextField(handler.PlatformView));
            EditorHandler.Mapper.AppendToMapping("MoneyLeftAlignment", (handler, _) => AlignTextField(handler.PlatformView));
            PickerHandler.Mapper.AppendToMapping("MoneyLeftAlignment", (handler, _) => AlignTextField(handler.PlatformView));
#endif
            builder
                .UseMauiApp<App>()
                .UseMaterialMauiIcons()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<FinancialForecastService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<BackupService>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<RecoveryPage>();
            builder.Services.AddTransient<TransactionFormPage>();
            builder.Services.AddTransient<CategoryManagementPage>();
            builder.Services.AddTransient<SubcategoryManagementPage>();
            builder.Services.AddTransient<SupplierManagementPage>();
            builder.Services.AddTransient<CardManagementPage>();
            builder.Services.AddTransient<AccountsPayablePage>();
            builder.Services.AddTransient<IncomeListPage>();
            builder.Services.AddTransient<IncomeFormPage>();
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
