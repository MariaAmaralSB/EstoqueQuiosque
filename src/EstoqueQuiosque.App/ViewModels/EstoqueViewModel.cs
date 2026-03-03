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

    // Cache local dos produtos — filtro roda aqui, sem ir à rede
    private List<Produto> _todosProdutos = [];

    private string _textoBusca = string.Empty;
    private string _categoriaSelecionada = "Todas as Categorias";
    private string _mensagemStatus = "Carregando produtos...";

    public EstoqueViewModel(EstoqueService estoqueService, CadastroProdutoViewModel cadastroViewModel)
    {
        _estoqueService = estoqueService;
        _cadastroViewModel = cadastroViewModel;

        // Só busca da rede quando os dados mudam no serviço
        _estoqueService.DadosAtualizados += () => _ = Task.Run(BuscarDoBancoAsync);

        RemoverProdutoCommand = new Command<Produto>(async produto => await RemoverProdutoAsync(produto));
        EditarProdutoCommand = new Command<Produto>(EditarProduto);
        RegistrarVendaCommand = new Command<Produto>(async produto => await RegistrarVendaAsync(produto));

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
                AplicarFiltros();   // só filtra local, sem rede
        }
    }

    public string CategoriaSelecionada
    {
        get => _categoriaSelecionada;
        set
        {
            if (SetProperty(ref _categoriaSelecionada, value))
                AplicarFiltros();   // só filtra local, sem rede
        }
    }

    public string MensagemStatus
    {
        get => _mensagemStatus;
        set => SetProperty(ref _mensagemStatus, value);
    }

    public ICommand RemoverProdutoCommand { get; }
    public ICommand EditarProdutoCommand { get; }
    public ICommand RegistrarVendaCommand { get; }

    // Vai à rede, atualiza o cache e depois filtra
    private async Task BuscarDoBancoAsync()
    {
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
    }

    // Reconstrói a lista de categorias usando o cache local
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

        // Altera o campo diretamente — sem passar pelo setter — para não disparar AplicarFiltros de novo
        if (!Categorias.Contains(_categoriaSelecionada))
            _categoriaSelecionada = "Todas as Categorias";
    }

    // Filtra o cache local — sem chamada de rede
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
            $"Deseja remover o produto '{produto.Nome}'?",
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
                $"Disponível: {produto.QuantidadeAtual} {produto.Unidade}",
                "OK");
            return;
        }

        try
        {
            await _estoqueService.RegistrarSaidaAsync(produto.Id, quantidade, "Venda");
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
