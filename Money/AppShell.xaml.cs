namespace Money
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        public void SetMainPage(MainPage page)
        {
            MainShellContent.Content = page;
        }
    }
}
