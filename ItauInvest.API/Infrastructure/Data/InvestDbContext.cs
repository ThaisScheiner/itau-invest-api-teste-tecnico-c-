using ItauInvest.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItauInvest.API.Infrastructure.Data
{
    public class InvestDbContext : DbContext
    {
        public InvestDbContext(DbContextOptions<InvestDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Ativo> Ativos => Set<Ativo>();
        public DbSet<Operacao> Operacoes => Set<Operacao>();
        public DbSet<Cotacao> Cotacoes => Set<Cotacao>();
        public DbSet<Posicao> Posicoes => Set<Posicao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aqui definimos o nome da tabela que será usada no banco para cada entidade.
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Ativo>().ToTable("ativos");
            modelBuilder.Entity<Operacao>().ToTable("operacoes");
            modelBuilder.Entity<Cotacao>().ToTable("cotacoes");
            modelBuilder.Entity<Posicao>().ToTable("posicoes");

            // Cria um índice composto na tabela de operações.
            // Isso acelera drasticamente as consultas que filtram por usuário, ativo e data.
            modelBuilder.Entity<Operacao>()
                .HasIndex(o => new { o.UsuarioId, o.AtivoId, o.DataHora })
                .HasDatabaseName("IX_Operacoes_Usuario_Ativo_Data");
        }
    }
}
