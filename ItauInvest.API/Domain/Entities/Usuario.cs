namespace ItauInvest.API.Domain.Entities
{
    public class Usuario
    {
        public long Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public decimal PercentualCorretagem { get; set; }
    }
}
