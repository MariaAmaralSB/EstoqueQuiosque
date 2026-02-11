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
    private Produto? _produtoSelecionado;
    private int _quantidadeMovimento = 1;
    private string _observacao = string.Empty;
    private string _mensagemStatus = "Pronto para registrar movimentos.";

    public EstoqueViewModel(EstoqueService estoqueService)
    {
        _estoqueService = estoqueService;
        _estoqueService.DadosAtualizados += AtualizarDados;

        RegistrarEntradaCommand = new Command(RegistrarEntrada, PodeRegistrarMovimento);
        RegistrarSaidaCommand = new Command(RegistrarSaida, PodeRegistrarMovimento);
        AtualizarDados();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Produto> Produtos { get; } = [];
    public ObservableCollection<MovimentoEstoque> MovimentosRecentes { get; } = [];

    public Produto? ProdutoSelecionado
    {
        get => _produtoSelecionado;
        set
        {
            if (SetProperty(ref _produtoSelecionado, value))
            {
                AtualizarComandos();
            }
        }
    }

    public int QuantidadeMovimento
    {
        get => _quantidadeMovimento;
        set
        {
            if (SetProperty(ref _quantidadeMovimento, value))
            {
                AtualizarComandos();
            }
        }
    }

    public string Observacao
    {
        get => _observacao;
        set => SetProperty(ref _observacao, value);
    }

    public string MensagemStatus
    {
        get => _mensagemStatus;
        set => SetProperty(ref _mensagemStatus, value);
    }

    public ICommand RegistrarEntradaCommand { get; }
    public ICommand RegistrarSaidaCommand { get; }

    private void AtualizarDados()
    {
        var produtoSelecionadoId = ProdutoSelecionado?.Id;

        Produtos.Clear();
        foreach (var produto in _estoqueService.ListarProdutos())
        {
            Produtos.Add(produto);
        }

        MovimentosRecentes.Clear();
        foreach (var movimento in _estoqueService.ListarMovimentos())
        {
            MovimentosRecentes.Add(movimento);
        }

        ProdutoSelecionado = Produtos.FirstOrDefault(p => p.Id == produtoSelecionadoId) ?? Produtos.FirstOrDefault();
        AtualizarComandos();
    }

    private bool PodeRegistrarMovimento() => ProdutoSelecionado is not null && QuantidadeMovimento > 0;

    private void RegistrarEntrada()
    {
        if (ProdutoSelecionado is null)
        {
            return;
        }

        _estoqueService.RegistrarEntrada(ProdutoSelecionado.Id, QuantidadeMovimento, Observacao);
        MensagemStatus = $"Entrada registrada para {ProdutoSelecionado.Nome}.";
        Observacao = string.Empty;
    }

    private void RegistrarSaida()
    {
        if (ProdutoSelecionado is null)
        {
            return;
        }

        try
        {
            _estoqueService.RegistrarSaida(ProdutoSelecionado.Id, QuantidadeMovimento, Observacao);
            MensagemStatus = $"Saída registrada para {ProdutoSelecionado.Nome}.";
            Observacao = string.Empty;
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

    private void AtualizarComandos()
    {
        (RegistrarEntradaCommand as Command)?.ChangeCanExecute();
        (RegistrarSaidaCommand as Command)?.ChangeCanExecute();
    }
}
