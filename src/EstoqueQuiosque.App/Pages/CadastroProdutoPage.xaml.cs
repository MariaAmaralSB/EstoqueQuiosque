using EstoqueQuiosque.App.ViewModels;

namespace EstoqueQuiosque.App.Pages;

public partial class CadastroProdutoPage : ContentPage
{
    public CadastroProdutoPage(CadastroProdutoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
