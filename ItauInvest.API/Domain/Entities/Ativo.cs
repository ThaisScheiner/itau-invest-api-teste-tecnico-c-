namespace ItauInvest.API.Domain.Entities
{
    public class Ativo
    {
        public long Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nome { get; set; } = null!;
    }
}
