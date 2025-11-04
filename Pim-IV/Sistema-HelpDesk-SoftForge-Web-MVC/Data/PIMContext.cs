using Microsoft.EntityFrameworkCore;
using PIM.Models;
using System.Linq;
using System.Threading;

namespace PIM.Data
{
    public class PIMContext : DbContext
    {
        public PIMContext(DbContextOptions<PIMContext> options) : base(options) { }

        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PerfilUsuario> PerfisUsuario { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Prioridade> Prioridades { get; set; }
        public DbSet<SLA> SLAs { get; set; }
        public DbSet<StatusChamado> StatusChamados { get; set; }
        public DbSet<BaseConhecimento> BasesConhecimento { get; set; }
        public DbSet<Chamado> Chamados { get; set; }
        public DbSet<Atendimento> Atendimentos { get; set; }
        public DbSet<Anexo> Anexos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<HistoricoChamado> HistoricosChamado { get; set; }
        public DbSet<LogAcesso> LogsAcesso { get; set; }
        public DbSet<IAAnalise> IAAnalises { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Departamento 1:N Usuario
            modelBuilder.Entity<Departamento>()
                .HasMany(d => d.Usuarios)
                .WithOne(u => u.Departamento)
                .HasForeignKey(u => u.DepartamentoId);

            // PerfilUsuario 1:N Usuario
            modelBuilder.Entity<PerfilUsuario>()
                .HasMany(p => p.Usuarios)
                .WithOne(u => u.PerfilUsuario)
                .HasForeignKey(u => u.PerfilId);

            // Usuario 1:1 Funcionario
            modelBuilder.Entity<Funcionario>()
                .HasOne(f => f.Usuario)
                .WithOne()
                .HasForeignKey<Funcionario>(f => f.UsuarioId);

            // Categoria 1:N BaseConhecimento
            modelBuilder.Entity<Categoria>()
                .HasMany(c => c.BaseConhecimentos)
                .WithOne(b => b.Categoria)
                .HasForeignKey(b => b.CategoriaId);

            // Categoria 1:N Chamado
            modelBuilder.Entity<Categoria>()
                .HasMany(c => c.Chamados)
                .WithOne(ch => ch.Categoria)
                .HasForeignKey(ch => ch.CategoriaId);

            // Prioridade 1:N SLA
            modelBuilder.Entity<Prioridade>()
                .HasMany(p => p.SLAs)
                .WithOne(s => s.Prioridade)
                .HasForeignKey(s => s.PrioridadeId);

            // Prioridade 1:N Chamado
            modelBuilder.Entity<Prioridade>()
                .HasMany(p => p.Chamados)
                .WithOne(ch => ch.Prioridade)
                .HasForeignKey(ch => ch.PrioridadeId);

            // SLA 1:N Chamado
            modelBuilder.Entity<SLA>()
                .HasMany(s => s.Chamados)
                .WithOne(ch => ch.SLA)
                .HasForeignKey(ch => ch.SLAId);

            // StatusChamado 1:N Chamado
            modelBuilder.Entity<StatusChamado>()
                .HasMany(s => s.Chamados)
                .WithOne(ch => ch.StatusChamado)
                .HasForeignKey(ch => ch.StatusId);

            // Usuario 1:N BaseConhecimento (criador)
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.BaseConhecimentosCriados)
                .WithOne(b => b.UsuarioCriador)
                .HasForeignKey(b => b.UsuarioCriadorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chamado 1:N Atendimento
            modelBuilder.Entity<Chamado>()
                .HasMany(c => c.Atendimentos)
                .WithOne(a => a.Chamado)
                .HasForeignKey(a => a.ChamadoId);

            // Usuario 1:N Atendimento (técnico)
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.AtendimentosTecnico)
                .WithOne(a => a.UsuarioTecnico)
                .HasForeignKey(a => a.UsuarioTecnicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chamado 1:N Anexo
            modelBuilder.Entity<Chamado>()
                .HasMany(c => c.Anexos)
                .WithOne(a => a.Chamado)
                .HasForeignKey(a => a.ChamadoId);

            // Usuario 1:N Anexo
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Anexos)
                .WithOne(a => a.Usuario)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chamado 1:1 Avaliacao
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.Avaliacao)
                .WithOne(a => a.Chamado)
                .HasForeignKey<Avaliacao>(a => a.ChamadoId);

            // Usuario 1:N Avaliacao (solicitante)
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.AvaliacoesSolicitante)
                .WithOne(a => a.UsuarioSolicitante)
                .HasForeignKey(a => a.UsuarioSolicitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chamado 1:N HistoricoChamado
            modelBuilder.Entity<Chamado>()
                .HasMany(c => c.HistoricosChamado)
                .WithOne(h => h.Chamado)
                .HasForeignKey(h => h.ChamadoId);

            // Usuario 1:N HistoricoChamado
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.HistoricosChamado)
                .WithOne(h => h.Usuario)
                .HasForeignKey(h => h.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // StatusChamado 1:N HistoricoChamado (status anterior)
            modelBuilder.Entity<StatusChamado>()
                .HasMany(s => s.HistoricosComoAnterior)
                .WithOne(h => h.StatusAnterior)
                .HasForeignKey(h => h.StatusAnteriorId)
                .OnDelete(DeleteBehavior.Restrict);

            // StatusChamado 1:N HistoricoChamado (status novo)
            modelBuilder.Entity<StatusChamado>()
                .HasMany(s => s.HistoricosComoNovo)
                .WithOne(h => h.StatusNovo)
                .HasForeignKey(h => h.StatusNovoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario 1:N LogAcesso
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.LogsAcesso)
                .WithOne(l => l.Usuario)
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chamado 1:N IAAnalise
            modelBuilder.Entity<Chamado>()
                .HasMany(c => c.IAAnalises)
                .WithOne(ia => ia.Chamado)
                .HasForeignKey(ia => ia.ChamadoId);

            base.OnModelCreating(modelBuilder);
        }
    }
}