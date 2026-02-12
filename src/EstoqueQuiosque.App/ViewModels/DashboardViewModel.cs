using System.ComponentModel;
using System.Runtime.CompilerServices;
using EstoqueQuiosque.App.Services;

namespace EstoqueQuiosque.App.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private readonly EstoqueService _estoqueService;

    private int _totalProdutos;
    private decimal _valorEmEstoque;
    private int _produtosAtivos;
    private int _estoqueBaixo;

    public DashboardViewModel(EstoqueService estoqueService)
    {
        _estoqueService = estoqueService;
        _estoqueService.DadosAtualizados += AtualizarIndicadores;
        AtualizarIndicadores();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int TotalProdutos
    {
        get => _totalProdutos;
        set => SetProperty(ref _totalProdutos, value);
    }

    public decimal ValorEmEstoque
    {
        get => _valorEmEstoque;
        set => SetProperty(ref _valorEmEstoque, value);
    }

    public int ProdutosAtivos
    {
        get => _produtosAtivos;
        set => SetProperty(ref _produtosAtivos, value);
    }

    public int EstoqueBaixo
    {
        get => _estoqueBaixo;
        set => SetProperty(ref _estoqueBaixo, value);
    }

    private void AtualizarIndicadores()
    {
        var produtos = _estoqueService.ListarProdutos();

        TotalProdutos = produtos.Count;
        ProdutosAtivos = produtos.Count(p => p.QuantidadeAtual > 0);
        EstoqueBaixo = produtos.Count(p => p.AbaixoDoMinimo);
        ValorEmEstoque = produtos.Sum(p => p.ValorEmEstoque);
    }

    private void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return;
        }

        backingStore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
