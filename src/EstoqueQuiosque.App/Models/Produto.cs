namespace EstoqueQuiosque.App.Models;

public class Produto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Unidade { get; set; } = "un";
    public int QuantidadeAtual { get; set; }
    public int EstoqueMinimo { get; set; }
    public decimal CustoUnitario { get; set; }

    public bool AbaixoDoMinimo => QuantidadeAtual <= EstoqueMinimo;
}
