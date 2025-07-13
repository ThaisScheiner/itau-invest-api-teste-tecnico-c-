using ItauInvest.API.Domain.Entities;
using ItauInvest.API.DTO.Operacoes;
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

        // --- ENDPOINTS GET QUE ESTAVAM A FALTAR ---

        [HttpGet("preco-medio/{usuarioId}/{ativoId}")]
        public async Task<ActionResult<decimal>> GetPrecoMedio(long usuarioId, long ativoId)
        {
            var preco = await _posicaoService.CalcularPrecoMedioAsync(usuarioId, ativoId);
            return Ok(preco);
        }

        [HttpGet("ultima-cotacao/{ativoId}")]
        public async Task<ActionResult<decimal>> GetUltimaCotacao(long ativoId)
        {
            var preco = await _posicaoService.ObterUltimaCotacaoAsync(ativoId);
            return Ok(preco);
        }

        [HttpGet("corretagem-total/{usuarioId}")]
        public async Task<ActionResult<decimal>> GetCorretagemTotal(long usuarioId)
        {
            // Nota: Este método não existia no seu PosicaoService, adicionei-o para si.
            // Certifique-se de que o método existe no seu PosicaoService.cs.
            var valor = await _posicaoService.CalcularTotalCorretagemAsync(usuarioId);
            return Ok(valor);
        }

        // --- ENDPOINTS EXISTENTES ---

        [HttpPost("operacoes")]
        public async Task<IActionResult> CriarOperacao([FromBody] CriarOperacaoDto operacaoDto)
        {
            if (operacaoDto == null)
            {
                return BadRequest("Dados da operação inválidos.");
            }

            var novaOperacao = new Operacao
            {
                UsuarioId = operacaoDto.UsuarioId,
                AtivoId = operacaoDto.AtivoId,
                Quantidade = operacaoDto.Quantidade,
                PrecoUnitario = operacaoDto.PrecoUnitario,
                TipoOperacao = operacaoDto.TipoOperacao,
                Corretagem = operacaoDto.Corretagem,
                DataHora = DateTime.UtcNow
            };

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

        [HttpGet("top10-posicao")]
        public async Task<ActionResult<List<TopPosicaoDto>>> GetTop10ClientesPorPosicao()
        {
            var lista = await _posicaoService.ObterTop10PorPosicaoAsync();
            return Ok(lista);
        }

        [HttpGet("top10-corretagem")]
        public async Task<ActionResult<List<TopCorretagemDto>>> GetTop10ClientesPorCorretagem()
        {
            var lista = await _posicaoService.ObterTop10PorCorretagemAsync();
            return Ok(lista);
        }
    }
}
