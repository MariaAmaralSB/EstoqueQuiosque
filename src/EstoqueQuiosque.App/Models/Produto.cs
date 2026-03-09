using Postgrest.Attributes;
using Postgrest.Models;

namespace EstoqueQuiosque.App.Models;

[Table("produtos")]
public class Produto : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Column("codigo")]
    public string Codigo { get; set; } = string.Empty;

    [Column("categoria")]
    public string Categoria { get; set; } = "Geral";

    [Column("unidade")]
    public string Unidade { get; set; } = "un";

    [Column("quantidade_atual")]
    public int QuantidadeAtual { get; set; }

    [Column("estoque_minimo")]
    public int EstoqueMinimo { get; set; }

    [Column("custo_unitario")]
    public decimal CustoUnitario { get; set; }

    [Column("preco_venda")]
    public decimal PrecoVenda { get; set; }

    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonIgnore]
    public string QuantidadeFormatada => $"{QuantidadeAtual} {Unidade}";

    [Newtonsoft.Json.JsonIgnore]
    public bool AbaixoDoMinimo => QuantidadeAtual <= EstoqueMinimo;

    [Newtonsoft.Json.JsonIgnore]
    public decimal ValorEmEstoque => QuantidadeAtual * CustoUnitario;

    [Newtonsoft.Json.JsonIgnore]
    public double ProgressoEstoque =>
        EstoqueMinimo > 0 ? Math.Min(1.0, (double)QuantidadeAtual / EstoqueMinimo) : 0;

    [Newtonsoft.Json.JsonIgnore]
    public string ResumoCategoriaMinimo => $"{Categoria} · min: {EstoqueMinimo} {Unidade}";
}
