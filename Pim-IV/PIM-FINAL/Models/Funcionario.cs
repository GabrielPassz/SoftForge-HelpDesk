using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_FINAL.Models
{
    public class Funcionario
    {
        [Key]
        [Column("funcionario_id")]
        public int FuncionarioId { get; set; }

        [ForeignKey("Usuario")]
        [Column("usuario_id")]
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Column("nome")]
        public string? Nome { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("data_contratacao")]
        public DateTime? DataContratacao { get; set; }

        [Column("cargo")]
        public string? Cargo { get; set; }

        [Column("matricula")]
        public string? Matricula { get; set; }

        [Column("data_demissao")]
        public DateTime? DataDemissao { get; set; }
    }
}