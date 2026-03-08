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
    private readonly CadastroProdutoViewModel _cadastroViewModel;

    private List<Produto> _todosProdutos = [];

    private string _textoBusca = string.Empty;
    private string _categoriaSelecionada = "Todas as Categorias";
    private string _mensagemStatus = "Carregando produtos...";
    private bool _isCarregando;
    private bool _filtroEstoqueBaixo;

    public EstoqueViewModel(EstoqueService estoqueService, CadastroProdutoViewModel cadastroViewModel)
    {
        _estoqueService = estoqueService;
        _cadastroViewModel = cadastroViewModel;

        _estoqueService.DadosAtualizados += () => _ = Task.Run(BuscarDoBancoAsync);

        RemoverProdutoCommand = new Command<Produto>(async produto => await RemoverProdutoAsync(produto));
        EditarProdutoCommand = new Command<Produto>(EditarProduto);
        RegistrarVendaCommand = new Command<Produto>(async produto => await RegistrarVendaAsync(produto));
        RegistrarEntradaCommand = new Command<Produto>(async produto => await RegistrarEntradaAsync(produto));
        ToggleFiltroEstoqueBaixoCommand = new Command(() => FiltroEstoqueBaixo = !FiltroEstoqueBaixo);

        _ = Task.Run(BuscarDoBancoAsync);
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
                AplicarFiltros();
        }
    }

    public string CategoriaSelecionada
    {
        get => _categoriaSelecionada;
        set
        {
            if (SetProperty(ref _categoriaSelecionada, value))
                AplicarFiltros();
        }
    }

    public string MensagemStatus
    {
        get => _mensagemStatus;
        set => SetProperty(ref _mensagemStatus, value);
    }

    public bool IsCarregando
    {
        get => _isCarregando;
        set => SetProperty(ref _isCarregando, value);
    }

    public bool FiltroEstoqueBaixo
    {
        get => _filtroEstoqueBaixo;
        set
        {
            if (SetProperty(ref _filtroEstoqueBaixo, value))
            {
                OnPropertyChanged(nameof(FiltroEstoqueBaixoCorFundo));
                AplicarFiltros();
            }
        }
    }

    public Color FiltroEstoqueBaixoCorFundo =>
        _filtroEstoqueBaixo ? Color.FromArgb("#5C2008") : Colors.Transparent;

    public ICommand RemoverProdutoCommand { get; }
    public ICommand EditarProdutoCommand { get; }
    public ICommand RegistrarVendaCommand { get; }
    public ICommand RegistrarEntradaCommand { get; }
    public ICommand ToggleFiltroEstoqueBaixoCommand { get; }

    private async Task BuscarDoBancoAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(() => IsCarregando = true);
        try
        {
            var todos = await _estoqueService.ListarProdutosAsync();
            _todosProdutos = todos.ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AtualizarCategorias();
                AplicarFiltros();
            });
        }
        catch
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                MensagemStatus = "Erro ao carregar produtos. Verifique a conexão.");
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsCarregando = false);
        }
    }

    private void AtualizarCategorias()
    {
        var categorias = _todosProdutos
            .Select(p => p.Categoria)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();

        Categorias.Clear();
        Categorias.Add("Todas as Categorias");
        foreach (var cat in categorias)
            Categorias.Add(cat);

        if (!Categorias.Contains(_categoriaSelecionada))
            _categoriaSelecionada = "Todas as Categorias";
    }

    private void AplicarFiltros()
    {
        var filtrados = _todosProdutos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(_textoBusca))
        {
            filtrados = filtrados.Where(p =>
                p.Nome.Contains(_textoBusca, StringComparison.OrdinalIgnoreCase) ||
                p.Codigo.Contains(_textoBusca, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(_categoriaSelecionada, "Todas as Categorias", StringComparison.OrdinalIgnoreCase))
        {
            filtrados = filtrados.Where(p =>
                string.Equals(p.Categoria, _categoriaSelecionada, StringComparison.OrdinalIgnoreCase));
        }

        if (_filtroEstoqueBaixo)
            filtrados = filtrados.Where(p => p.AbaixoDoMinimo);

        var lista = filtrados.ToList();

        ProdutosFiltrados.Clear();
        foreach (var produto in lista)
            ProdutosFiltrados.Add(produto);

        MensagemStatus = $"{lista.Count} produto(s) encontrado(s)";
    }

    private async Task RemoverProdutoAsync(Produto? produto)
    {
        if (produto is null)
            return;

        var confirmado = await Application.Current!.MainPage!.DisplayAlert(
            "Confirmar exclusão",
            $"Produto: {produto.Nome}\nCódigo: {produto.Codigo}\nQuantidade: {produto.QuantidadeFormatada}\nValor em estoque: R$ {produto.ValorEmEstoque:N2}\n\nEssa ação não pode ser desfeita.",
            "Remover",
            "Cancelar");

        if (!confirmado)
            return;

        try
        {
            await _estoqueService.RemoverProdutoAsync(produto.Id);
        }
        catch
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                MensagemStatus = "Erro ao remover produto. Verifique a conexão.");
        }
    }

    private async Task RegistrarVendaAsync(Produto? produto)
    {
        if (produto is null)
            return;

        var input = await Application.Current!.MainPage!.DisplayPromptAsync(
            "Registrar Venda",
            $"{produto.Nome}\nEstoque atual: {produto.QuantidadeAtual} {produto.Unidade}\n\nQuantidade vendida:",
            "Confirmar",
            "Cancelar",
            keyboard: Microsoft.Maui.Keyboard.Numeric,
            initialValue: "1");

        if (input is null)
            return;

        if (!int.TryParse(input, out int quantidade) || quantidade <= 0)
        {
            await Application.Current.MainPage!.DisplayAlert("Quantidade inválida", "Informe um número maior que zero.", "OK");
            return;
        }

        if (quantidade > produto.QuantidadeAtual)
        {
            await Application.Current.MainPage!.DisplayAlert(
                "Estoque insuficiente",
                $"Disponível: {produto.QuantidadeFormatada}",
                "OK");
            return;
        }

        var obs = await Application.Current.MainPage!.DisplayPromptAsync(
            "Observação (opcional)",
            "Adicione uma nota para esta venda:",
            "Confirmar",
            "Pular",
            placeholder: "Ex: Venda balcão");
        var observacao = string.IsNullOrWhiteSpace(obs) ? "Venda" : obs.Trim();

        try
        {
            await _estoqueService.RegistrarSaidaAsync(produto.Id, quantidade, observacao);
        }
        catch (InvalidOperationException ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() => MensagemStatus = ex.Message);
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() => MensagemStatus = $"Erro ao registrar venda: {ex.Message}");
        }
    }

    private async Task RegistrarEntradaAsync(Produto? produto)
    {
        if (produto is null)
            return;

        var input = await Application.Current!.MainPage!.DisplayPromptAsync(
            "Registrar Entrada",
            $"{produto.Nome}\nEstoque atual: {produto.QuantidadeFormatada}\n\nQuantidade recebida:",
            "Confirmar",
            "Cancelar",
            keyboard: Microsoft.Maui.Keyboard.Numeric,
            initialValue: "1");

        if (input is null)
            return;

        if (!int.TryParse(input, out int quantidade) || quantidade <= 0)
        {
            await Application.Current.MainPage!.DisplayAlert("Quantidade inválida", "Informe um número maior que zero.", "OK");
            return;
        }

        var obs = await Application.Current.MainPage!.DisplayPromptAsync(
            "Observação (opcional)",
            "Adicione uma nota para esta entrada:",
            "Confirmar",
            "Pular",
            placeholder: "Ex: Reposição de estoque");
        var observacao = string.IsNullOrWhiteSpace(obs) ? "Entrada de estoque" : obs.Trim();

        try
        {
            await _estoqueService.RegistrarEntradaAsync(produto.Id, quantidade, observacao);
        }
        catch (InvalidOperationException ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() => MensagemStatus = ex.Message);
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() => MensagemStatus = $"Erro ao registrar entrada: {ex.Message}");
        }
    }

    private void EditarProduto(Produto? produto)
    {
        if (produto is null)
            return;

        _cadastroViewModel.ProdutoSelecionadoParaEdicao = produto;
        Shell.Current.GoToAsync("//CadastroProdutoPage");
    }

    private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
