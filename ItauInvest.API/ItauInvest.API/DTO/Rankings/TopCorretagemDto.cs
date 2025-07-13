namespace ItauInvest.API.DTO.Rankings
{
    // DTO para o ranking de corretagens
    public class TopCorretagemDto
    {
        public long UsuarioId { get; set; }
        public decimal TotalCorretagem { get; set; }
    }
}
