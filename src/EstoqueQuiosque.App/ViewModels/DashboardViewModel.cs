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
        _estoqueService.DadosAtualizados += () => _ = Task.Run(AtualizarIndicadoresAsync);
        _ = Task.Run(AtualizarIndicadoresAsync);
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

    private async Task AtualizarIndicadoresAsync()
    {
        try
        {
            var produtos = await _estoqueService.ListarProdutosAsync();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                TotalProdutos = produtos.Count;
                ProdutosAtivos = produtos.Count(p => p.QuantidadeAtual > 0);
                EstoqueBaixo = produtos.Count(p => p.AbaixoDoMinimo);
                ValorEmEstoque = produtos.Sum(p => p.ValorEmEstoque);
            });
        }
        catch
        {
            // Dashboard não atualiza se não houver conexão
        }
    }

    private void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return;

        backingStore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
