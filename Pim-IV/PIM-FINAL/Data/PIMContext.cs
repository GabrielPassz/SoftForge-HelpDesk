using Microsoft.EntityFrameworkCore; // EF Core ORM
using PIM_FINAL.Models; // Entidades do domínio

// PIMContext: expõe DbSets e configura mapeamentos/relacionamentos para PostgreSQL (schema public).
namespace PIM_FINAL.Data
{
    public class PIMContext : DbContext
    {
        public PIMContext(DbContextOptions<PIMContext> options) : base(options) { }

        // DbSets: coleções que representam tabelas
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PerfilUsuario> PerfisUsuario { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Prioridade> Prioridades { get; set; }
        public DbSet<Sla> SLAs { get; set; }
        public DbSet<StatusChamado> StatusChamados { get; set; }
        public DbSet<BaseConhecimento> BasesConhecimento { get; set; }
        public DbSet<Chamado> Chamados { get; set; }
        public DbSet<Atendimento> Atendimentos { get; set; }
        public DbSet<Anexo> Anexos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<HistoricoChamado> HistoricosChamado { get; set; }
        public DbSet<LogAcesso> LogsAcesso { get; set; }
        public DbSet<IaAnalise> IAAnalises { get; set; }
        public DbSet<Comunicacao> Comunicacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tabelas (schema public)
            modelBuilder.Entity<Departamento>().ToTable("departamento", "public");
            modelBuilder.Entity<Usuario>().ToTable("usuario", "public");
            modelBuilder.Entity<PerfilUsuario>().ToTable("perfil_usuario", "public");
            modelBuilder.Entity<Funcionario>().ToTable("funcionario", "public");
            modelBuilder.Entity<Categoria>().ToTable("categoria", "public");
            modelBuilder.Entity<Prioridade>().ToTable("prioridade", "public");
            modelBuilder.Entity<Sla>().ToTable("sla", "public");
            modelBuilder.Entity<StatusChamado>().ToTable("status_chamado", "public");
            modelBuilder.Entity<BaseConhecimento>().ToTable("base_conhecimento", "public");
            modelBuilder.Entity<Chamado>().ToTable("chamado", "public");
            modelBuilder.Entity<Atendimento>().ToTable("atendimento", "public");
            modelBuilder.Entity<Anexo>().ToTable("anexo", "public");
            modelBuilder.Entity<Avaliacao>().ToTable("avaliacao", "public");
            modelBuilder.Entity<HistoricoChamado>().ToTable("historico_chamado", "public");
            modelBuilder.Entity<LogAcesso>().ToTable("log_acesso", "public");
            modelBuilder.Entity<IaAnalise>().ToTable("ia_analise", "public");
            modelBuilder.Entity<Comunicacao>().ToTable("comunicacao", "public");

            // Ajustes de propriedades (exemplo: Comunicacao)
            modelBuilder.Entity<Comunicacao>(entity =>
            {
                entity.Property(e => e.ComunicacaoId).HasColumnName("comunicacao_id");
                entity.Property(e => e.ChamadoId).HasColumnName("chamado_id");
                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
                entity.Property(e => e.Mensagem).HasColumnName("mensagem");
                entity.Property(e => e.DataEnvio).HasColumnName("data_envio");
            });

            // Relacionamentos e comportamentos de deleção
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Departamento)
                .WithMany(d => d.Usuarios)
                .HasForeignKey(u => u.DepartamentoId)
                .OnDelete(DeleteBehavior.SetNull); // ao deletar depto, mantém usuário com null
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.PerfilUsuario)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(u => u.PerfilId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Funcionario>()
                .HasOne(f => f.Usuario)
                .WithOne()
                .HasForeignKey<Funcionario>(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<BaseConhecimento>()
                .HasOne(b => b.Categoria)
                .WithMany(c => c.BaseConhecimentos)
                .HasForeignKey(b => b.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<BaseConhecimento>()
                .HasOne(b => b.UsuarioCriador)
                .WithMany(u => u.BaseConhecimentosCriados)
                .HasForeignKey(b => b.UsuarioCriadorId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.UsuarioSolicitante)
                .WithMany(u => u.ChamadosSolicitados)
                .HasForeignKey(c => c.UsuarioSolicitanteId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.TecnicoResponsavel)
                .WithMany(u => u.ChamadosComoTecnico)
                .HasForeignKey(c => c.TecnicoResponsavelId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.Categoria)
                .WithMany(cat => cat.Chamados)
                .HasForeignKey(c => c.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.Prioridade)
                .WithMany(p => p.Chamados)
                .HasForeignKey(c => c.PrioridadeId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.StatusChamado)
                .WithMany()
                .HasForeignKey(c => c.StatusId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.Sla)
                .WithMany(s => s.Chamados)
                .HasForeignKey(c => c.SlaId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Atendimento>()
                .HasOne(a => a.Chamado)
                .WithMany(ch => ch.Atendimentos)
                .HasForeignKey(a => a.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade); // ao apagar chamado, apaga atendimentos
            modelBuilder.Entity<Atendimento>()
                .HasOne(a => a.UsuarioTecnico)
                .WithMany(u => u.AtendimentosTecnico)
                .HasForeignKey(a => a.UsuarioTecnicoId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Anexo>()
                .HasOne(a => a.Chamado)
                .WithMany(ch => ch.Anexos)
                .HasForeignKey(a => a.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Anexo>()
                .HasOne(a => a.Usuario)
                .WithMany(u => u.Anexos)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Chamado)
                .WithOne(ch => ch.Avaliacao)
                .HasForeignKey<Avaliacao>(a => a.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.UsuarioSolicitante)
                .WithMany(u => u.AvaliacoesSolicitante)
                .HasForeignKey(a => a.UsuarioSolicitanteId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<HistoricoChamado>()
                .HasOne(h => h.Chamado)
                .WithMany(ch => ch.HistoricosChamado)
                .HasForeignKey(h => h.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<HistoricoChamado>()
                .HasOne(h => h.Usuario)
                .WithMany(u => u.HistoricosChamado)
                .HasForeignKey(h => h.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<HistoricoChamado>()
                .HasOne(h => h.StatusAnterior)
                .WithMany()
                .HasForeignKey(h => h.StatusAnteriorId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<HistoricoChamado>()
                .HasOne(h => h.StatusNovo)
                .WithMany()
                .HasForeignKey(h => h.StatusNovoId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<LogAcesso>()
                .HasOne(l => l.Usuario)
                .WithMany(u => u.LogsAcesso)
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<IaAnalise>()
                .HasOne(ia => ia.Chamado)
                .WithMany(ch => ch.IAAnalises)
                .HasForeignKey(ia => ia.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comunicacao>()
                .HasOne(c => c.Chamado)
                .WithMany(ch => ch.Comunicacoes)
                .HasForeignKey(c => c.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comunicacao>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Comunicacoes)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // Índice único (uma avaliação por chamado)
            modelBuilder.Entity<Avaliacao>()
                .HasIndex(a => a.ChamadoId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}