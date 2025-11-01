using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("prioridade")]
    public class Prioridade
    {
        [Key]
        [Column("prioridade_id")]
        public int PrioridadeId { get; set; }

        [Required]
        [Column("nome_prioridade")]
        public string NomePrioridade { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; }

        public ICollection<SLA> SLAs { get; set; }
        public ICollection<Chamado> Chamados { get; set; }
    }

}
