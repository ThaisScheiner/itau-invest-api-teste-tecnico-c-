using ItauInvest.API.Domain.Entities;
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

        // Este método é chamado pelo Kafka Worker para atualizar a posição no banco.
        public async Task RecalcularESalvarPosicaoAsync(long usuarioId, long ativoId)
        {
            // 1. Calcula a nova posição em memória
            var novaPosicao = await CalcularPosicaoAsync(usuarioId, ativoId);
            if (novaPosicao == null) return;

            // 2. Procura por uma posição existente no banco para este usuário/ativo
            var posicaoExistente = await _context.Posicoes
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.AtivoId == ativoId);

            // 3. Se existir, atualiza. Se não, adiciona.
            if (posicaoExistente != null)
            {
                posicaoExistente.Quantidade = novaPosicao.Quantidade;
                posicaoExistente.PrecoMedio = novaPosicao.PrecoMedio;
                posicaoExistente.PL = novaPosicao.PL;
                _context.Posicoes.Update(posicaoExistente);
            }
            else
            {
                // Adiciona a nova posição se ela não existir
                await _context.Posicoes.AddAsync(novaPosicao);
            }

            // 4. Salva as alterações no banco de dados.
            await _context.SaveChangesAsync();
        }

        // --- MÉTODO FALTANTE ADICIONADO AQUI ---
        // Encontra todos os usuários que têm uma posição em um determinado ativo e recalcula
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

        public async Task<decimal> CalcularTotalCorretagemAsync(long usuarioId)
        {
            return await _context.Operacoes
                .Where(o => o.UsuarioId == usuarioId)
                .SumAsync(o => o.Corretagem);
        }

        public async Task<List<(long usuarioId, decimal valorTotal)>> ObterTop10PorPosicaoAsync()
        {
            var agrupado = await _context.Operacoes
                .Where(o => o.TipoOperacao == "Compra")
                .GroupBy(o => o.UsuarioId)
                .Select(g => new
                {
                    UsuarioId = g.Key,
                    ValorTotal = g.Sum(o => o.Quantidade * o.PrecoUnitario)
                })
                .OrderByDescending(x => x.ValorTotal)
                .Take(10)
                .ToListAsync();

            return agrupado.Select(x => (x.UsuarioId, x.ValorTotal)).ToList();
        }

        public async Task<List<(long usuarioId, decimal totalCorretagem)>> ObterTop10PorCorretagemAsync()
        {
            var agrupado = await _context.Operacoes
                .GroupBy(o => o.UsuarioId)
                .Select(g => new
                {
                    UsuarioId = g.Key,
                    Total = g.Sum(o => o.Corretagem)
                })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToListAsync();

            return agrupado.Select(x => (x.UsuarioId, x.Total)).ToList();
        }
    }
}