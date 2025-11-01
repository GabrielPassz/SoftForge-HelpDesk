using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("base_conhecimento")]
    public class BaseConhecimento
    {
        [Key]
        [Column("base_id")]
        public int BaseId { get; set; }

        [Required]
        [Column("titulo")]
        public string Titulo { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; }

        [Column("solucao")]
        public string Solucao { get; set; }

        [Column("data_criacao")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Column("aprovado")]
        public bool Aprovado { get; set; }

        [ForeignKey("Categoria")]
        [Column("categoria_id")]
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        [ForeignKey("Usuario")]
        [Column("usuario_criador_id")]
        public int UsuarioCriadorId { get; set; }
        public Usuario UsuarioCriador { get; set; }
    }

}
