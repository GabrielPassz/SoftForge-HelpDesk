using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("sla")]
    public class SLA
    {
        [Key]
        [Column("sla_id")]
        public int SLAId { get; set; }

        [Required]
        [Column("nome_sla")]
        public string NomeSLA { get; set; }

        [Required]
        [Column("tempo_primeira_resposta")]
        public int TempoPrimeiraResposta { get; set; }

        [Required]
        [Column("tempo_maximo_resolucao")]
        public int TempoMaximoResolucao { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; }

        [ForeignKey("Prioridade")]
        [Column("prioridade_id")]
        public int PrioridadeId { get; set; }
        public Prioridade Prioridade { get; set; }

        public ICollection<Chamado> Chamados { get; set; }
    }

}
