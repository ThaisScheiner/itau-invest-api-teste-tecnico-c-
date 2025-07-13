namespace ItauInvest.API.Domain.Entities
{
    public class Posicao
    {
        public long Id { get; set; }
        public long UsuarioId { get; set; }
        public long AtivoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal PL { get; set; } // Profit and Loss
    }
}
