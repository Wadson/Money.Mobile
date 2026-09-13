using Money.Services;
namespace Money.Views.Management;
public partial class SubcategoryManagementPage : CategoryManagementBase
{
    public SubcategoryManagementPage(DatabaseService database) : base(database, false) { InitializeComponent(); }
}
