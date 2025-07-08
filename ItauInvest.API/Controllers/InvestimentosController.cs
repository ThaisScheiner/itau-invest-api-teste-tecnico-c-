using ItauInvest.API.Domain.Entities;
using ItauInvest.API.DTO.Rankings;
using ItauInvest.API.Infrastructure.Data;
using ItauInvest.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItauInvest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestimentosController : ControllerBase
    {
        private readonly PosicaoService _posicaoService;
        private readonly InvestDbContext _context;

        public InvestimentosController(PosicaoService posicaoService, InvestDbContext context)
        {
            _posicaoService = posicaoService;
            _context = context;
        }

        // --- MÉTODO ATUALIZADO ---
        // A assinatura do método agora usa o DTO importado.
        [HttpGet("top10-posicao")]
        public async Task<ActionResult<List<TopPosicaoDto>>> GetTop10ClientesPorPosicao()
        {
            var lista = await _posicaoService.ObterTop10PorPosicaoAsync();
            return Ok(lista);
        }

        // --- MÉTODO ATUALIZADO ---
        // A assinatura do método agora usa o DTO importado.
        [HttpGet("top10-corretagem")]
        public async Task<ActionResult<List<TopCorretagemDto>>> GetTop10ClientesPorCorretagem()
        {
            var lista = await _posicaoService.ObterTop10PorCorretagemAsync();
            return Ok(lista);
        }

        // --- O resto dos seus endpoints permanecem iguais ---
        [HttpPost("operacoes")]
        public async Task<IActionResult> CriarOperacao([FromBody] Operacao novaOperacao)
        {
            if (novaOperacao == null)
            {
                return BadRequest("Dados da operação inválidos.");
            }

            novaOperacao.DataHora = DateTime.UtcNow;

            _context.Operacoes.Add(novaOperacao);
            await _context.SaveChangesAsync();

            await _posicaoService.RecalcularESalvarPosicaoAsync(novaOperacao.UsuarioId, novaOperacao.AtivoId);

            return CreatedAtAction(nameof(GetPosicao), new { usuarioId = novaOperacao.UsuarioId, ativoId = novaOperacao.AtivoId }, novaOperacao);
        }

        [HttpGet("posicao/{usuarioId}/{ativoId}")]
        public async Task<ActionResult<Posicao>> GetPosicao(long usuarioId, long ativoId)
        {
            var posicao = await _context.Posicoes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.AtivoId == ativoId);

            if (posicao == null)
            {
                return NotFound($"Nenhuma posição encontrada para o utilizador {usuarioId} com o ativo {ativoId}.");
            }

            return Ok(posicao);
        }
    }
}
