using System.ComponentModel.DataAnnotations;

namespace ItauInvest.API.DTO.Operacoes
{
    public class CriarOperacaoDto
    {
        [Required]
        public long UsuarioId { get; set; }

        [Required]
        public long AtivoId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal PrecoUnitario { get; set; }

        [Required]
        public string TipoOperacao { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Corretagem { get; set; }
    }
}
