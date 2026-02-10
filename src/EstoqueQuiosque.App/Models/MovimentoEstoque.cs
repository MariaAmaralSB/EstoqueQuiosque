namespace EstoqueQuiosque.App.Models;

public class MovimentoEstoque
{
    public DateTime Data { get; set; } = DateTime.Now;
    public string ProdutoNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Observacao { get; set; } = string.Empty;

    public string Resumo => $"{Tipo} - {ProdutoNome}";
}
