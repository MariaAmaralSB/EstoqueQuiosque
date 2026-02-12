using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EstoqueQuiosque.App.Models;
using EstoqueQuiosque.App.Services;

namespace EstoqueQuiosque.App.ViewModels;

public class EstoqueViewModel : INotifyPropertyChanged
{
    private readonly EstoqueService _estoqueService;
    private string _textoBusca = string.Empty;
    private string _categoriaSelecionada = "Todas as Categorias";
    private string _mensagemStatus = "Visualize e gerencie seu estoque.";

    public EstoqueViewModel(EstoqueService estoqueService)
    {
        _estoqueService = estoqueService;
        _estoqueService.DadosAtualizados += AtualizarDados;

        RemoverProdutoCommand = new Command<Produto>(RemoverProduto);
        AtualizarDados();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Produto> ProdutosFiltrados { get; } = [];
    public ObservableCollection<string> Categorias { get; } = [];

    public string TextoBusca
    {
        get => _textoBusca;
        set
        {
            if (SetProperty(ref _textoBusca, value))
            {
                AplicarFiltros();
            }
        }
    }

    public string CategoriaSelecionada
    {
        get => _categoriaSelecionada;
        set
        {
            if (SetProperty(ref _categoriaSelecionada, value))
            {
                AplicarFiltros();
            }
        }
    }

    public string MensagemStatus
    {
        get => _mensagemStatus;
        set => SetProperty(ref _mensagemStatus, value);
    }

    public ICommand RemoverProdutoCommand { get; }

    private void AtualizarDados()
    {
        var categorias = _estoqueService.ListarProdutos()
            .Select(p => p.Categoria)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();

        Categorias.Clear();
        Categorias.Add("Todas as Categorias");
        foreach (var categoria in categorias)
        {
            Categorias.Add(categoria);
        }

        if (!Categorias.Contains(CategoriaSelecionada))
        {
            CategoriaSelecionada = "Todas as Categorias";
        }

        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var produtos = _estoqueService.ListarProdutos().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(TextoBusca))
        {
            produtos = produtos.Where(p =>
                p.Nome.Contains(TextoBusca, StringComparison.OrdinalIgnoreCase) ||
                p.Codigo.Contains(TextoBusca, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(CategoriaSelecionada, "Todas as Categorias", StringComparison.OrdinalIgnoreCase))
        {
            produtos = produtos.Where(p => string.Equals(p.Categoria, CategoriaSelecionada, StringComparison.OrdinalIgnoreCase));
        }

        ProdutosFiltrados.Clear();
        foreach (var produto in produtos)
        {
            ProdutosFiltrados.Add(produto);
        }
    }

    private void RemoverProduto(Produto? produto)
    {
        if (produto is null)
        {
            return;
        }

        try
        {
            _estoqueService.RemoverProduto(produto.Id);
            MensagemStatus = $"Produto '{produto.Nome}' removido com sucesso.";
        }
        catch (InvalidOperationException ex)
        {
            MensagemStatus = ex.Message;
        }
    }

    private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
