using EstoqueQuiosque.App.Models;

namespace EstoqueQuiosque.App.Services;

public class EstoqueService
{
    private readonly List<Produto> _produtos =
    [
        new Produto
        {
            Nome = "Notebook Dell Inspiron",
            Codigo = "PROD-001",
            Categoria = "Informática",
            Unidade = "un",
            QuantidadeAtual = 5,
            EstoqueMinimo = 3,
            CustoUnitario = 2800m,
            PrecoVenda = 3500m,
            Descricao = "Notebook para uso corporativo"
        },
        new Produto
        {
            Nome = "Mouse Logitech MX",
            Codigo = "PROD-002",
            Categoria = "Eletrônicos",
            Unidade = "un",
            QuantidadeAtual = 2,
            EstoqueMinimo = 3,
            CustoUnitario = 180m,
            PrecoVenda = 250m,
            Descricao = "Mouse sem fio ergonômico"
        },
        new Produto
        {
            Nome = "Teclado Mecânico RGB",
            Codigo = "PROD-003",
            Categoria = "Eletrônicos",
            Unidade = "un",
            QuantidadeAtual = 15,
            EstoqueMinimo = 4,
            CustoUnitario = 320m,
            PrecoVenda = 450m,
            Descricao = "Teclado com iluminação RGB"
        }
    ];

    private readonly List<MovimentoEstoque> _movimentos = [];

    public event Action? DadosAtualizados;

    public IReadOnlyList<Produto> ListarProdutos() => _produtos.OrderBy(p => p.Nome).ToList();

    public IReadOnlyList<MovimentoEstoque> ListarMovimentos() => _movimentos
        .OrderByDescending(m => m.Data)
        .Take(20)
        .ToList();

    public void CadastrarProduto(
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

        if (_produtos.Any(p => string.Equals(p.Codigo, codigoNormalizado, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Já existe um produto com esse código.");
        }

        _produtos.Add(new Produto
        {
            Nome = nomeNormalizado,
            Codigo = codigoNormalizado,
            Categoria = string.IsNullOrWhiteSpace(categoria) ? "Geral" : categoria.Trim(),
            Unidade = "un",
            QuantidadeAtual = quantidadeInicial,
            EstoqueMinimo = estoqueMinimo,
            CustoUnitario = custoUnitario,
            PrecoVenda = precoVenda,
            Descricao = descricao.Trim()
        });

        NotificarAtualizacao();
    }

    public void AtualizarProduto(
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
        var produto = _produtos.FirstOrDefault(p => p.Id == produtoId)
                     ?? throw new InvalidOperationException("Produto não encontrado para atualização.");

        var nomeNormalizado = nome.Trim();
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();

        ValidarDadosProduto(nomeNormalizado, codigoNormalizado, quantidadeAtual, estoqueMinimo, custoUnitario, precoVenda);

        if (_produtos.Any(p => p.Id != produtoId && string.Equals(p.Codigo, codigoNormalizado, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Já existe outro produto com esse código.");
        }

        produto.Nome = nomeNormalizado;
        produto.Codigo = codigoNormalizado;
        produto.Categoria = string.IsNullOrWhiteSpace(categoria) ? "Geral" : categoria.Trim();
        produto.QuantidadeAtual = quantidadeAtual;
        produto.EstoqueMinimo = estoqueMinimo;
        produto.CustoUnitario = custoUnitario;
        produto.PrecoVenda = precoVenda;
        produto.Descricao = descricao.Trim();

        NotificarAtualizacao();
    }

    public void RemoverProduto(Guid produtoId)
    {
        var produto = _produtos.FirstOrDefault(p => p.Id == produtoId)
                     ?? throw new InvalidOperationException("Produto não encontrado para exclusão.");

        _produtos.Remove(produto);
        NotificarAtualizacao();
    }

    public void RegistrarEntrada(Guid produtoId, int quantidade, string observacao)
    {
        var produto = _produtos.FirstOrDefault(p => p.Id == produtoId)
                     ?? throw new InvalidOperationException("Produto não encontrado.");

        produto.QuantidadeAtual += quantidade;

        _movimentos.Add(new MovimentoEstoque
        {
            ProdutoNome = produto.Nome,
            Quantidade = quantidade,
            Tipo = "Entrada",
            Observacao = observacao
        });

        NotificarAtualizacao();
    }

    public void RegistrarSaida(Guid produtoId, int quantidade, string observacao)
    {
        var produto = _produtos.FirstOrDefault(p => p.Id == produtoId)
                     ?? throw new InvalidOperationException("Produto não encontrado.");

        if (quantidade > produto.QuantidadeAtual)
        {
            throw new InvalidOperationException("Quantidade de saída maior que o estoque disponível.");
        }

        produto.QuantidadeAtual -= quantidade;

        _movimentos.Add(new MovimentoEstoque
        {
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
        {
            throw new InvalidOperationException("Informe o nome do produto.");
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new InvalidOperationException("Informe o código do produto.");
        }

        if (quantidade < 0 || estoqueMinimo < 0)
        {
            throw new InvalidOperationException("Quantidade e estoque mínimo devem ser maiores ou iguais a zero.");
        }

        if (custoUnitario < 0 || precoVenda < 0)
        {
            throw new InvalidOperationException("Os preços devem ser maiores ou iguais a zero.");
        }
    }

    private void NotificarAtualizacao() => DadosAtualizados?.Invoke();
}
