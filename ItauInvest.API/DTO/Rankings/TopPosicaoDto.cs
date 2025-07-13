namespace ItauInvest.API.DTO.Rankings
{
    // DTO (Data Transfer Object) para o ranking de posições
    public class TopPosicaoDto
    {
        public long UsuarioId { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
