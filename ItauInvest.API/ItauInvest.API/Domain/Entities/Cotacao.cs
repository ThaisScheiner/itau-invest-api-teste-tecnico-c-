namespace ItauInvest.API.Domain.Entities
{
    public class Cotacao
    {
        public long Id { get; set; }
        public long AtivoId { get; set; }
        public decimal PrecoUnitario { get; set; }
        public DateTime DataHora { get; set; }
    }
}
