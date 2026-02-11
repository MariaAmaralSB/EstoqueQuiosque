using EstoqueQuiosque.App.Models;

namespace EstoqueQuiosque.App.Services;

public class EstoqueService
{
    private readonly List<Produto> _produtos =
    [
        new Produto { Nome = "Água 500ml", Unidade = "un", QuantidadeAtual = 60, EstoqueMinimo = 20, CustoUnitario = 1.50m },
        new Produto { Nome = "Água de Coco", Unidade = "un", QuantidadeAtual = 25, EstoqueMinimo = 15, CustoUnitario = 4.00m },
        new Produto { Nome = "Protetor Solar Sache", Unidade = "un", QuantidadeAtual = 10, EstoqueMinimo = 12, CustoUnitario = 3.75m }
    ];

    private readonly List<MovimentoEstoque> _movimentos = [];

    public event Action? DadosAtualizados;

    public IReadOnlyList<Produto> ListarProdutos() => _produtos.OrderBy(p => p.Nome).ToList();

    public IReadOnlyList<MovimentoEstoque> ListarMovimentos() => _movimentos
        .OrderByDescending(m => m.Data)
        .Take(20)
        .ToList();

    public void CadastrarProduto(string nome, string unidade, int quantidadeInicial, int estoqueMinimo, decimal custoUnitario)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Informe o nome do produto.");
        }

        if (quantidadeInicial < 0 || estoqueMinimo < 0)
        {
            throw new InvalidOperationException("Quantidade e estoque mínimo devem ser maiores ou iguais a zero.");
        }

        if (custoUnitario < 0)
        {
            throw new InvalidOperationException("O custo unitário deve ser maior ou igual a zero.");
        }

        if (_produtos.Any(p => string.Equals(p.Nome, nome.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Já existe um produto com esse nome.");
        }

        _produtos.Add(new Produto
        {
            Nome = nome.Trim(),
            Unidade = string.IsNullOrWhiteSpace(unidade) ? "un" : unidade.Trim().ToLowerInvariant(),
            QuantidadeAtual = quantidadeInicial,
            EstoqueMinimo = estoqueMinimo,
            CustoUnitario = custoUnitario
        });

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

    private void NotificarAtualizacao() => DadosAtualizados?.Invoke();
}
