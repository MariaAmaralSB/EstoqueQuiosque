using Postgrest.Attributes;
using Postgrest.Models;

namespace EstoqueQuiosque.App.Models;

[Table("movimentos_estoque")]
public class MovimentoEstoque : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("data")]
    public DateTime Data { get; set; } = DateTime.UtcNow;

    [Column("produto_nome")]
    public string ProdutoNome { get; set; } = string.Empty;

    [Column("quantidade")]
    public int Quantidade { get; set; }

    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Column("observacao")]
    public string Observacao { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonIgnore]
    public string Resumo => $"{Tipo} — {ProdutoNome}";

    [Newtonsoft.Json.JsonIgnore]
    public string DeltaQuantidade => Tipo == "Entrada" ? $"+{Quantidade}" : $"-{Quantidade}";
}
