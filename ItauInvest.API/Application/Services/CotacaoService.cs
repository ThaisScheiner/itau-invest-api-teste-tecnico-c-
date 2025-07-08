using ItauInvest.API.Domain.Entities;
using ItauInvest.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ItauInvest.Application.Services;

public class CotacaoService
{
    private readonly InvestDbContext _context;
    private readonly PosicaoService _posicaoService;

    public CotacaoService(InvestDbContext context, PosicaoService posicaoService)
    {
        _context = context;
        _posicaoService = posicaoService;
    }

    // Método para salvar uma nova cotação recebida via Kafka
    public async Task SalvarCotacaoAsync(string codigoAtivo, decimal preco)
    {
        var ativo = await _context.Ativos.FirstOrDefaultAsync(a => a.Codigo == codigoAtivo);

        if (ativo == null)
        {
            // Opcional: Logar que o ativo não foi encontrado
            return;
        }

        var novaCotacao = new Cotacao
        {
            AtivoId = ativo.Id,
            PrecoUnitario = preco,
            DataHora = DateTime.UtcNow
        };

        await _context.Cotacoes.AddAsync(novaCotacao);
        await _context.SaveChangesAsync();

        // BÔNUS: Após salvar uma nova cotação, vamos recalcular a posição
        // de todos os usuários que possuem este ativo, para atualizar o P/L (Profit/Loss).
        await _posicaoService.RecalcularPosicoesPorAtivoAsync(ativo.Id);
    }
}