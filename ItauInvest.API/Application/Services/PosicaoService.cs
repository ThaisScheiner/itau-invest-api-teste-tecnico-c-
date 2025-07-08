using ItauInvest.API.Domain.Entities;
using ItauInvest.API.DTO.Rankings;
using ItauInvest.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ItauInvest.Application.Services
{
    public class PosicaoService
    {
        private readonly InvestDbContext _context;

        public PosicaoService(InvestDbContext context)
        {
            _context = context;
        }

        // --- MÉTODO ATUALIZADO ---
        // Agora retorna uma lista de TopPosicaoDto a partir do seu novo namespace.
        public async Task<List<TopPosicaoDto>> ObterTop10PorPosicaoAsync()
        {
            return await _context.Operacoes
                .Where(o => o.TipoOperacao == "Compra")
                .GroupBy(o => o.UsuarioId)
                .Select(g => new TopPosicaoDto // Cria o objeto DTO
                {
                    UsuarioId = g.Key,
                    ValorTotal = g.Sum(o => o.Quantidade * o.PrecoUnitario)
                })
                .OrderByDescending(x => x.ValorTotal)
                .Take(10)
                .ToListAsync();
        }

        // --- MÉTODO ATUALIZADO ---
        // Agora retorna uma lista de TopCorretagemDto a partir do seu novo namespace.
        public async Task<List<TopCorretagemDto>> ObterTop10PorCorretagemAsync()
        {
            return await _context.Operacoes
                .GroupBy(o => o.UsuarioId)
                .Select(g => new TopCorretagemDto // Cria o objeto DTO
                {
                    UsuarioId = g.Key,
                    TotalCorretagem = g.Sum(o => o.Corretagem)
                })
                .OrderByDescending(x => x.TotalCorretagem)
                .Take(10)
                .ToListAsync();
        }

        // --- O resto dos seus métodos permanecem iguais ---
        public async Task RecalcularESalvarPosicaoAsync(long usuarioId, long ativoId)
        {
            var novaPosicao = await CalcularPosicaoAsync(usuarioId, ativoId);
            if (novaPosicao == null) return;

            var posicaoExistente = await _context.Posicoes
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.AtivoId == ativoId);

            if (posicaoExistente != null)
            {
                posicaoExistente.Quantidade = novaPosicao.Quantidade;
                posicaoExistente.PrecoMedio = novaPosicao.PrecoMedio;
                posicaoExistente.PL = novaPosicao.PL;
                _context.Posicoes.Update(posicaoExistente);
            }
            else
            {
                await _context.Posicoes.AddAsync(novaPosicao);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RecalcularPosicoesPorAtivoAsync(long ativoId)
        {
            var usuariosComPosicao = await _context.Posicoes
                .Where(p => p.AtivoId == ativoId && p.Quantidade > 0)
                .Select(p => p.UsuarioId)
                .Distinct()
                .ToListAsync();

            foreach (var usuarioId in usuariosComPosicao)
            {
                await RecalcularESalvarPosicaoAsync(usuarioId, ativoId);
            }
        }

        public async Task<Posicao?> CalcularPosicaoAsync(long usuarioId, long ativoId)
        {
            var operacoes = await _context.Operacoes
                .Where(o => o.UsuarioId == usuarioId && o.AtivoId == ativoId)
                .ToListAsync();

            if (!operacoes.Any()) return null;

            var compras = operacoes.Where(o => o.TipoOperacao == "Compra");
            var vendas = operacoes.Where(o => o.TipoOperacao == "Venda");
            var qtdTotal = compras.Sum(c => c.Quantidade) - vendas.Sum(v => v.Quantidade);

            if (qtdTotal <= 0)
            {
                return new Posicao { UsuarioId = usuarioId, AtivoId = ativoId, Quantidade = 0, PrecoMedio = 0, PL = 0 };
            }

            var precoMedio = await CalcularPrecoMedioAsync(usuarioId, ativoId);
            var ultimaCotacao = await ObterUltimaCotacaoAsync(ativoId);
            var pl = (ultimaCotacao - precoMedio) * qtdTotal;

            return new Posicao
            {
                UsuarioId = usuarioId,
                AtivoId = ativoId,
                Quantidade = qtdTotal,
                PrecoMedio = precoMedio,
                PL = pl
            };
        }

        public async Task<decimal> CalcularPrecoMedioAsync(long usuarioId, long ativoId)
        {
            var compras = await _context.Operacoes
                .Where(o => o.UsuarioId == usuarioId && o.AtivoId == ativoId && o.TipoOperacao == "Compra")
                .ToListAsync();

            if (!compras.Any()) return 0;
            var totalValor = compras.Sum(c => c.PrecoUnitario * c.Quantidade);
            var totalQtd = compras.Sum(c => c.Quantidade);
            return totalQtd == 0 ? 0 : totalValor / totalQtd;
        }

        public async Task<decimal> ObterUltimaCotacaoAsync(long ativoId)
        {
            var cotacao = await _context.Cotacoes
                .Where(c => c.AtivoId == ativoId)
                .OrderByDescending(c => c.DataHora)
                .FirstOrDefaultAsync();
            return cotacao?.PrecoUnitario ?? 0;
        }
    }
}