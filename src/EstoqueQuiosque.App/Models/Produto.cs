namespace EstoqueQuiosque.App.Models;

public class Produto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Categoria { get; set; } = "Geral";
    public string Unidade { get; set; } = "un";
    public int QuantidadeAtual { get; set; }
    public int EstoqueMinimo { get; set; }
    public decimal CustoUnitario { get; set; }
    public decimal PrecoVenda { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public bool AbaixoDoMinimo => QuantidadeAtual <= EstoqueMinimo;
    public decimal ValorEmEstoque => QuantidadeAtual * CustoUnitario;
}
