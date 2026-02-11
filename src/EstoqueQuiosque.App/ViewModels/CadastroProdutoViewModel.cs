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
    private Produto? _produtoSelecionadoParaEdicao;
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
        AtualizarProdutoCommand = new Command(AtualizarProduto, PodeAtualizarProduto);
        LimparFormularioCommand = new Command(LimparFormulario);
        AtualizarLista();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Produto> ProdutosCadastrados { get; } = [];

    public Produto? ProdutoSelecionadoParaEdicao
    {
        get => _produtoSelecionadoParaEdicao;
        set
        {
            if (SetProperty(ref _produtoSelecionadoParaEdicao, value))
            {
                if (value is not null)
                {
                    NomeProduto = value.Nome;
                    Unidade = value.Unidade;
                    QuantidadeInicial = value.QuantidadeAtual;
                    EstoqueMinimo = value.EstoqueMinimo;
                    CustoUnitario = value.CustoUnitario;
                    MensagemStatus = $"Editando produto: {value.Nome}";
                }

                AtualizarComandos();
            }
        }
    }

    public string NomeProduto
    {
        get => _nomeProduto;
        set
        {
            if (SetProperty(ref _nomeProduto, value))
            {
                AtualizarComandos();
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

    public bool EstaEmEdicao => ProdutoSelecionadoParaEdicao is not null;

    public ICommand CadastrarProdutoCommand { get; }
    public ICommand AtualizarProdutoCommand { get; }
    public ICommand LimparFormularioCommand { get; }

    private void AtualizarLista()
    {
        var produtoSelecionadoId = ProdutoSelecionadoParaEdicao?.Id;

        ProdutosCadastrados.Clear();
        foreach (var produto in _estoqueService.ListarProdutos())
        {
            ProdutosCadastrados.Add(produto);
        }

        ProdutoSelecionadoParaEdicao = ProdutosCadastrados.FirstOrDefault(p => p.Id == produtoSelecionadoId);
        AtualizarComandos();
    }

    private bool PodeCadastrarProduto() => !string.IsNullOrWhiteSpace(NomeProduto) && !EstaEmEdicao;

    private bool PodeAtualizarProduto() => !string.IsNullOrWhiteSpace(NomeProduto) && EstaEmEdicao;

    private void CadastrarProduto()
    {
        try
        {
            _estoqueService.CadastrarProduto(NomeProduto, Unidade, QuantidadeInicial, EstoqueMinimo, CustoUnitario);
            MensagemStatus = $"Produto '{NomeProduto.Trim()}' cadastrado com sucesso.";
            LimparFormulario();
        }
        catch (InvalidOperationException ex)
        {
            MensagemStatus = ex.Message;
        }
    }

    private void AtualizarProduto()
    {
        if (ProdutoSelecionadoParaEdicao is null)
        {
            return;
        }

        try
        {
            _estoqueService.AtualizarProduto(
                ProdutoSelecionadoParaEdicao.Id,
                NomeProduto,
                Unidade,
                QuantidadeInicial,
                EstoqueMinimo,
                CustoUnitario);

            MensagemStatus = $"Produto '{NomeProduto.Trim()}' atualizado com sucesso.";
            LimparFormulario();
        }
        catch (InvalidOperationException ex)
        {
            MensagemStatus = ex.Message;
        }
    }

    private void LimparFormulario()
    {
        ProdutoSelecionadoParaEdicao = null;
        NomeProduto = string.Empty;
        Unidade = "un";
        QuantidadeInicial = 0;
        EstoqueMinimo = 0;
        CustoUnitario = 0;
        OnPropertyChanged(nameof(EstaEmEdicao));
        AtualizarComandos();
    }

    private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        OnPropertyChanged(propertyName);

        if (propertyName == nameof(ProdutoSelecionadoParaEdicao))
        {
            OnPropertyChanged(nameof(EstaEmEdicao));
        }

        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void AtualizarComandos()
    {
        (CadastrarProdutoCommand as Command)?.ChangeCanExecute();
        (AtualizarProdutoCommand as Command)?.ChangeCanExecute();
    }
}
