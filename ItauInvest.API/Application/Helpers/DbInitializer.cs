using ItauInvest.API.Domain.Entities;
using ItauInvest.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static void Seed(InvestDbContext context)
    {
        // Aplica migrations pendentes
        context.Database.Migrate();

        // Usuarios
        if (!context.Usuarios.Any())
        {
            var usuarios = new[]
            {
                    new Usuario { Nome = "João Silva", Email = "joao@itau.com", PercentualCorretagem = 0.01m },
                    new Usuario { Nome = "Maria Souza", Email = "maria@itau.com", PercentualCorretagem = 0.015m }
                };
            context.Usuarios.AddRange(usuarios);
            context.SaveChanges();
        }

        // Ativos
        if (!context.Ativos.Any())
        {
            var ativos = new[]
            {
                    new Ativo { Codigo = "PETR4", Nome = "Petrobras PN" },
                    new Ativo { Codigo = "VALE3", Nome = "Vale ON" }
                };
            context.Ativos.AddRange(ativos);
            context.SaveChanges();
        }

        // Operacoes
        if (!context.Operacoes.Any())
        {
            var usuario1 = context.Usuarios.First();
            var ativo1 = context.Ativos.First();

            var operacoes = new[]
            {
                    new Operacao
                    {
                        UsuarioId = usuario1.Id,
                        AtivoId = ativo1.Id,
                        Quantidade = 100,
                        PrecoUnitario = 25.50m,
                        TipoOperacao = "Compra",
                        Corretagem = 10m,
                        DataHora = DateTime.Now.AddDays(-10)
                    },
                    new Operacao
                    {
                        UsuarioId = usuario1.Id,
                        AtivoId = ativo1.Id,
                        Quantidade = 50,
                        PrecoUnitario = 27.00m,
                        TipoOperacao = "Compra",
                        Corretagem = 5m,
                        DataHora = DateTime.Now.AddDays(-5)
                    }
                };
            context.Operacoes.AddRange(operacoes);
            context.SaveChanges();
        }

        // Cotacoes
        if (!context.Cotacoes.Any())
        {
            var ativo1 = context.Ativos.First();

            var cotacoes = new[]
            {
                    new Cotacao
                    {
                        AtivoId = ativo1.Id,
                        PrecoUnitario = 28.00m,
                        DataHora = DateTime.Now.AddDays(-1)
                    },
                    new Cotacao
                    {
                        AtivoId = ativo1.Id,
                        PrecoUnitario = 29.00m,
                        DataHora = DateTime.Now
                    }
                };
            context.Cotacoes.AddRange(cotacoes);
            context.SaveChanges();
        }

        // Posicoes
        if (!context.Posicoes.Any())
        {
            var usuario1 = context.Usuarios.First();
            var ativo1 = context.Ativos.First();

            // Pega a cotação mais recente que acabamos de inserir para o cálculo do P/L
            var ultimaCotacao = context.Cotacoes
                .Where(c => c.AtivoId == ativo1.Id)
                .OrderByDescending(c => c.DataHora)
                .First();

            var precoMedio = 26.00m; // Mantendo o mesmo PM do seu exemplo
            var quantidade = 150;

            // CORREÇÃO: o P/L agora é calculado com base na última cotação do seed
            var plCalculado = (ultimaCotacao.PrecoUnitario - precoMedio) * quantidade;

            var posicoes = new[]
            {
        new Posicao
        {
            UsuarioId = usuario1.Id,
            AtivoId = ativo1.Id,
            Quantidade = quantidade,
            PrecoMedio = precoMedio,
            PL = plCalculado // Usa a variável calculada
        }
    };
            context.Posicoes.AddRange(posicoes);
            context.Posicoes.AddRange(posicoes);
            context.SaveChanges();
        }
    }
}