namespace Money.Models;

public sealed class Fornecedor
{
    public long IdFornecedor { get; set; }
    public long IdUsuario { get; set; }
    public string NomeFornecedor { get; set; } = string.Empty;
    public long Ativo { get; set; } = 1;
    public string DataCriacao { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    public bool IsActive => Ativo == 1;
    public string Status => IsActive ? "Ativo" : "Inativo";
}
