using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("avaliacao")]
    public class Avaliacao
    {
        [Key]
        [Column("avaliacao_id")]
        public int AvaliacaoId { get; set; }

        [ForeignKey("Chamado")]
        [Column("chamado_id")]
        public int ChamadoId { get; set; }
        public Chamado Chamado { get; set; }

        [ForeignKey("UsuarioSolicitante")]
        [Column("usuario_solicitante_id")]
        public int UsuarioSolicitanteId { get; set; }
        public Usuario UsuarioSolicitante { get; set; }

        [Range(1, 5)]
        [Column("nota")]
        public int Nota { get; set; }

        [Column("comentario")]
        public string Comentario { get; set; }

        [Column("data_avaliacao")]
        public DateTime DataAvaliacao { get; set; } = DateTime.Now;
    }

}
