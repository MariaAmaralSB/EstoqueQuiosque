using EstoqueQuiosque.App.Models;
using static Postgrest.Constants;

namespace EstoqueQuiosque.App.Services;

public class EstoqueService
{
    private readonly Supabase.Client _supabase;

    public event Action? DadosAtualizados;

    public EstoqueService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<IReadOnlyList<Produto>> ListarProdutosAsync()
    {
        var result = await _supabase.From<Produto>()
            .Order("nome", Ordering.Ascending)
            .Get();
        return result.Models;
    }

    public async Task<IReadOnlyList<MovimentoEstoque>> ListarMovimentosAsync()
    {
        var result = await _supabase.From<MovimentoEstoque>()
            .Order("data", Ordering.Descending)
            .Limit(20)
            .Get();
        return result.Models;
    }

    public async Task CadastrarProdutoAsync(
        string nome,
        string codigo,
        string categoria,
        int quantidadeInicial,
        int estoqueMinimo,
        decimal custoUnitario,
        decimal precoVenda,
        string descricao)
    {
        var nomeNormalizado = nome.Trim();
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();

        ValidarDadosProduto(nomeNormalizado, codigoNormalizado, quantidadeInicial, estoqueMinimo, custoUnitario, precoVenda);

        var existente = await _supabase.From<Produto>()
            .Filter("codigo", Operator.Equals, codigoNormalizado)
            .Get();

        if (existente.Models.Count > 0)
            throw new InvalidOperationException("Já existe um produto com esse código.");

        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = nomeNormalizado,
            Codigo = codigoNormalizado,
            Categoria = string.IsNullOrWhiteSpace(categoria) ? "Geral" : categoria.Trim(),
            Unidade = "un",
            QuantidadeAtual = quantidadeInicial,
            EstoqueMinimo = estoqueMinimo,
            CustoUnitario = custoUnitario,
            PrecoVenda = precoVenda,
            Descricao = descricao.Trim()
        };

        await _supabase.From<Produto>().Insert(produto);
        NotificarAtualizacao();
    }

    public async Task AtualizarProdutoAsync(
        Guid produtoId,
        string nome,
        string codigo,
        string categoria,
        int quantidadeAtual,
        int estoqueMinimo,
        decimal custoUnitario,
        decimal precoVenda,
        string descricao)
    {
        var nomeNormalizado = nome.Trim();
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();

        ValidarDadosProduto(nomeNormalizado, codigoNormalizado, quantidadeAtual, estoqueMinimo, custoUnitario, precoVenda);

        var duplicado = await _supabase.From<Produto>()
            .Filter("codigo", Operator.Equals, codigoNormalizado)
            .Filter("id", Operator.NotEqual, produtoId.ToString())
            .Get();

        if (duplicado.Models.Count > 0)
            throw new InvalidOperationException("Já existe outro produto com esse código.");

        var resultado = await _supabase.From<Produto>()
            .Filter("id", Operator.Equals, produtoId.ToString())
            .Get();

        var produto = resultado.Models.FirstOrDefault()
            ?? throw new InvalidOperationException("Produto não encontrado para atualização.");

        produto.Nome = nomeNormalizado;
        produto.Codigo = codigoNormalizado;
        produto.Categoria = string.IsNullOrWhiteSpace(categoria) ? "Geral" : categoria.Trim();
        produto.QuantidadeAtual = quantidadeAtual;
        produto.EstoqueMinimo = estoqueMinimo;
        produto.CustoUnitario = custoUnitario;
        produto.PrecoVenda = precoVenda;
        produto.Descricao = descricao.Trim();

        await _supabase.From<Produto>().Update(produto);
        NotificarAtualizacao();
    }

    public async Task RemoverProdutoAsync(Guid produtoId)
    {
        await _supabase.From<Produto>()
            .Filter("id", Operator.Equals, produtoId.ToString())
            .Delete();

        NotificarAtualizacao();
    }

    public async Task RegistrarEntradaAsync(Guid produtoId, int quantidade, string observacao)
    {
        var resultado = await _supabase.From<Produto>()
            .Filter("id", Operator.Equals, produtoId.ToString())
            .Get();

        var produto = resultado.Models.FirstOrDefault()
            ?? throw new InvalidOperationException("Produto não encontrado.");

        produto.QuantidadeAtual += quantidade;
        await _supabase.From<Produto>().Update(produto);

        await _supabase.From<MovimentoEstoque>().Insert(new MovimentoEstoque
        {
            Id = Guid.NewGuid(),
            ProdutoNome = produto.Nome,
            Quantidade = quantidade,
            Tipo = "Entrada",
            Observacao = observacao
        });

        NotificarAtualizacao();
    }

    public async Task RegistrarSaidaAsync(Guid produtoId, int quantidade, string observacao)
    {
        var resultado = await _supabase.From<Produto>()
            .Filter("id", Operator.Equals, produtoId.ToString())
            .Get();

        var produto = resultado.Models.FirstOrDefault()
            ?? throw new InvalidOperationException("Produto não encontrado.");

        if (quantidade > produto.QuantidadeAtual)
            throw new InvalidOperationException("Quantidade de saída maior que o estoque disponível.");

        produto.QuantidadeAtual -= quantidade;
        await _supabase.From<Produto>().Update(produto);

        await _supabase.From<MovimentoEstoque>().Insert(new MovimentoEstoque
        {
            Id = Guid.NewGuid(),
            ProdutoNome = produto.Nome,
            Quantidade = quantidade,
            Tipo = "Saída",
            Observacao = observacao
        });

        NotificarAtualizacao();
    }

    private static void ValidarDadosProduto(string nome, string codigo, int quantidade, int estoqueMinimo, decimal custoUnitario, decimal precoVenda)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new InvalidOperationException("Informe o nome do produto.");

        if (string.IsNullOrWhiteSpace(codigo))
            throw new InvalidOperationException("Informe o código do produto.");

        if (quantidade < 0 || estoqueMinimo < 0)
            throw new InvalidOperationException("Quantidade e estoque mínimo devem ser maiores ou iguais a zero.");

        if (custoUnitario < 0 || precoVenda < 0)
            throw new InvalidOperationException("Os preços devem ser maiores ou iguais a zero.");
    }

    private void NotificarAtualizacao() => DadosAtualizados?.Invoke();
}
