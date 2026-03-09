using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EstoqueQuiosque.App.Models;
using EstoqueQuiosque.App.Services;

namespace EstoqueQuiosque.App.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private readonly EstoqueService _estoqueService;

    private int _totalProdutos;
    private decimal _valorEmEstoque;
    private int _produtosAtivos;
    private int _estoqueBaixo;
    private bool _isCarregando;

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

    public bool IsCarregando
    {
        get => _isCarregando;
        set => SetProperty(ref _isCarregando, value);
    }

    public ObservableCollection<Produto> ProdutosEstoqueBaixoLista { get; } = [];
    public ObservableCollection<MovimentoEstoque> MovimentosRecentes { get; } = [];
    public ObservableCollection<CategoriaDistribuicao> CategoriasDistribuicao { get; } = [];

    public ICommand VerProdutosCommand { get; } =
        new Command(() => Shell.Current.GoToAsync("//EstoquePage"));
    public ICommand VerMovimentosCommand { get; } =
        new Command(() => Shell.Current.GoToAsync("//MovimentosPage"));

    private async Task AtualizarIndicadoresAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(() => IsCarregando = true);
        try
        {
            var produtos = await _estoqueService.ListarProdutosAsync();
            var movimentos = await _estoqueService.ListarMovimentosAsync();

            string[] cores = ["#63B3ED", "#4ADE80", "#FB923C", "#A78BFA", "#F87171", "#FCD34D"];

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                TotalProdutos = produtos.Count;
                ProdutosAtivos = produtos.Count(p => p.QuantidadeAtual > 0);
                EstoqueBaixo = produtos.Count(p => p.AbaixoDoMinimo);
                ValorEmEstoque = produtos.Sum(p => p.ValorEmEstoque);

                ProdutosEstoqueBaixoLista.Clear();
                produtos.Where(p => p.AbaixoDoMinimo).Take(5).ToList()
                    .ForEach(p => ProdutosEstoqueBaixoLista.Add(p));

                MovimentosRecentes.Clear();
                movimentos.Take(5).ToList().ForEach(m => MovimentosRecentes.Add(m));

                CategoriasDistribuicao.Clear();
                produtos.GroupBy(p => p.Categoria)
                    .Select((g, i) => new CategoriaDistribuicao
                    {
                        Nome = g.Key,
                        Quantidade = g.Count(),
                        Percentagem = produtos.Count > 0 ? (double)g.Count() / produtos.Count : 0,
                        Cor = cores[i % cores.Length]
                    }).ToList().ForEach(c => CategoriasDistribuicao.Add(c));
            });
        }
        catch
        {
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsCarregando = false);
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
