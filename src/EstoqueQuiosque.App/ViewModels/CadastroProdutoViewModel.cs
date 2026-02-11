using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EstoqueQuiosque.App.Models;
using EstoqueQuiosque.App.Services;

namespace EstoqueQuiosque.App.ViewModels;

public class CadastroProdutoViewModel : INotifyPropertyChanged
{
    private readonly EstoqueService _estoqueService;
    private string _nomeProduto = string.Empty;
    private string _unidade = "un";
    private int _quantidadeInicial;
    private int _estoqueMinimo;
    private decimal _custoUnitario;
    private string _mensagemStatus = "Cadastre um novo produto para iniciar o controle.";

    public CadastroProdutoViewModel(EstoqueService estoqueService)
    {
        _estoqueService = estoqueService;
        _estoqueService.DadosAtualizados += AtualizarLista;

        CadastrarProdutoCommand = new Command(CadastrarProduto, PodeCadastrarProduto);
        AtualizarLista();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Produto> ProdutosCadastrados { get; } = [];

    public string NomeProduto
    {
        get => _nomeProduto;
        set
        {
            if (SetProperty(ref _nomeProduto, value))
            {
                AtualizarComandoCadastro();
            }
        }
    }

    public string Unidade
    {
        get => _unidade;
        set => SetProperty(ref _unidade, value);
    }

    public int QuantidadeInicial
    {
        get => _quantidadeInicial;
        set => SetProperty(ref _quantidadeInicial, value);
    }

    public int EstoqueMinimo
    {
        get => _estoqueMinimo;
        set => SetProperty(ref _estoqueMinimo, value);
    }

    public decimal CustoUnitario
    {
        get => _custoUnitario;
        set => SetProperty(ref _custoUnitario, value);
    }

    public string MensagemStatus
    {
        get => _mensagemStatus;
        set => SetProperty(ref _mensagemStatus, value);
    }

    public ICommand CadastrarProdutoCommand { get; }

    private void AtualizarLista()
    {
        ProdutosCadastrados.Clear();
        foreach (var produto in _estoqueService.ListarProdutos())
        {
            ProdutosCadastrados.Add(produto);
        }
    }

    private bool PodeCadastrarProduto() => !string.IsNullOrWhiteSpace(NomeProduto);

    private void CadastrarProduto()
    {
        try
        {
            _estoqueService.CadastrarProduto(NomeProduto, Unidade, QuantidadeInicial, EstoqueMinimo, CustoUnitario);
            MensagemStatus = $"Produto '{NomeProduto.Trim()}' cadastrado com sucesso.";

            NomeProduto = string.Empty;
            Unidade = "un";
            QuantidadeInicial = 0;
            EstoqueMinimo = 0;
            CustoUnitario = 0;
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

    private void AtualizarComandoCadastro() => (CadastrarProdutoCommand as Command)?.ChangeCanExecute();
}
