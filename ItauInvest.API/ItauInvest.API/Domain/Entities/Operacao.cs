namespace ItauInvest.API.Domain.Entities
{
    public class Operacao
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public long AtivoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public string TipoOperacao { get; set; } = null!; // "Compra" ou "Venda"
        public decimal Corretagem { get; set; }
        public DateTime DataHora { get; set; }
    }
}
