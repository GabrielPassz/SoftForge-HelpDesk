using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("status_chamado")]
    public class StatusChamado
    {
        [Key]
        [Column("status_id")]
        public int StatusId { get; set; }

        [Required]
        [Column("nome_status")]
        public string NomeStatus { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; }

        public ICollection<Chamado> Chamados { get; set; }
        public ICollection<HistoricoChamado> HistoricosComoAnterior { get; set; }
        public ICollection<HistoricoChamado> HistoricosComoNovo { get; set; }
    }

}
