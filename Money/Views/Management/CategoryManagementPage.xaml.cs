using Money.Services;
namespace Money.Views.Management;
public partial class CategoryManagementPage : CategoryManagementBase
{
    public CategoryManagementPage(DatabaseService database) : base(database, true) { InitializeComponent(); }
}
