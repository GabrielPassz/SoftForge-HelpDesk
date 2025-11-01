using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("ia_analise")]
    public class IAAnalise
    {
        [Key]
        [Column("ia_id")]
        public int IAId { get; set; }

        [ForeignKey("Chamado")]
        [Column("chamado_id")]
        public int ChamadoId { get; set; }
        public Chamado Chamado { get; set; }

        [Column("categoria_prevista")]
        public string CategoriaPrevista { get; set; }

        [Column("confianca")]
        public decimal Confianca { get; set; }

        [Column("resultado_validado")]
        public bool ResultadoValidado { get; set; }

        [Column("comentario")]
        public string Comentario { get; set; }

        [Column("data_analise")]
        public DateTime DataAnalise { get; set; } = DateTime.Now;
    }

}
