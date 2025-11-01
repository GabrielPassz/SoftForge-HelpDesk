using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("chamado")]
    public class Chamado
    {
        [Key]
        [Column("chamado_id")]
        public int ChamadoId { get; set; }

        [Required]
        [Column("protocolo")]
        public string Protocolo { get; set; }

        [Required]
        [Column("titulo")]
        public string Titulo { get; set; }

        [Required]
        [Column("descricao")]
        public string Descricao { get; set; }

        [Column("data_abertura")]
        public DateTime DataAbertura { get; set; } = DateTime.Now;

        [Column("data_fechamento")]
        public DateTime? DataFechamento { get; set; }

        [Column("sla_atingido")]
        public bool SLAAtingido { get; set; }

        [ForeignKey("UsuarioSolicitante")]
        [Column("usuario_solicitante_id")]
        public int UsuarioSolicitanteId { get; set; }
        public Usuario UsuarioSolicitante { get; set; }

        [ForeignKey("TecnicoResponsavel")]
        [Column("tecnico_responsavel_id")]
        public int TecnicoResponsavelId { get; set; }
        public Usuario TecnicoResponsavel { get; set; }

        [ForeignKey("Categoria")]
        [Column("categoria_id")]
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        [ForeignKey("Prioridade")]
        [Column("prioridade_id")]
        public int PrioridadeId { get; set; }
        public Prioridade Prioridade { get; set; }

        [ForeignKey("StatusChamado")]
        [Column("status_id")]
        public int StatusId { get; set; }
        public StatusChamado StatusChamado { get; set; }

        [ForeignKey("SLA")]
        [Column("sla_id")]
        public int SLAId { get; set; }
        public SLA SLA { get; set; }

        public ICollection<Atendimento> Atendimentos { get; set; }
        public ICollection<Anexo> Anexos { get; set; }
        public Avaliacao Avaliacao { get; set; }
        public ICollection<IAAnalise> IAAnalises { get; set; }
        public ICollection<HistoricoChamado> HistoricosChamado { get; set; }
    }

}
