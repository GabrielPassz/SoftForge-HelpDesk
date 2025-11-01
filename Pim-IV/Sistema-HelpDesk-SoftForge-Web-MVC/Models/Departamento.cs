using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("departamento")]
    public class Departamento
    {
        [Key]
        [Column("departamento_id")]
        public int DepartamentoId { get; set; }

        [Required]
        [Column("nome_departamento")]
        [Display(Name = "Nome do Departamento")]
        public string NomeDepartamento { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
    }
}
