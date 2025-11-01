using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("perfil_usuario")]
    public class PerfilUsuario
    {
        [Key]
        [Column("perfil_id")]
        public int PerfilId { get; set; }

        [Required]
        [Column("nome_perfil")]
        [Display(Name = "Nome do Perfil")]
        public string NomePerfil { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
    }
}
