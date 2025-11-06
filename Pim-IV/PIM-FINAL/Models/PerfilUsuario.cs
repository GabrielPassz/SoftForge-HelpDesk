using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace PIM_FINAL.Models
{
    public class PerfilUsuario
    {
        [Key]
        [Column("perfil_id")]
        public int PerfilId { get; set; }

        [Required]
        [Column("nome_perfil")]
        public string? NomePerfil { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }

        // Navigation
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}