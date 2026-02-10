using EstoqueQuiosque.App.ViewModels;

namespace EstoqueQuiosque.App.Pages;

public partial class EstoquePage : ContentPage
{
    public EstoquePage(EstoqueViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
