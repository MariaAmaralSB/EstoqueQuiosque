using EstoqueQuiosque.App.ViewModels;

namespace EstoqueQuiosque.App.Pages;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
