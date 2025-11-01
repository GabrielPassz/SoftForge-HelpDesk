using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("anexo")]
    public class Anexo
    {
        [Key]
        [Column("anexo_id")]
        public int AnexoId { get; set; }

        [ForeignKey("Chamado")]
        [Column("chamado_id")]
        public int ChamadoId { get; set; }
        public Chamado Chamado { get; set; }

        [Required]
        [Column("nome_arquivo")]
        public string NomeArquivo { get; set; }

        [Column("tipo_arquivo")]
        public string TipoArquivo { get; set; }

        [Column("caminho_arquivo")]
        public string CaminhoArquivo { get; set; }

        [Column("data_upload")]
        public DateTime DataUpload { get; set; } = DateTime.Now;

        [ForeignKey("Usuario")]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }

}
