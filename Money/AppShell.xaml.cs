using Money.Views.Dashboard;
using Money.Views.Transactions;

namespace Money
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(IncomeListPage),typeof(IncomeListPage));
            Routing.RegisterRoute(nameof(IncomeFormPage),typeof(IncomeFormPage));
        }

        public void SetMainPage(MainPage page)
        {
            MainShellContent.Content = page;
        }
    }
}
