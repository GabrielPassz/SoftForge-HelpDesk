using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_FINAL.Models
{
    public class BaseConhecimento
    {
        [Key]
        [Column("base_id")]
        public int BaseId { get; set; }

        [Required]
        [Column("titulo")]
        public string? Titulo { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("solucao")]
        public string? Solucao { get; set; }

        [ForeignKey("Categoria")]
        [Column("categoria_id")]
        public int? CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        [ForeignKey("UsuarioCriador")]
        [Column("usuario_criador_id")]
        public int? UsuarioCriadorId { get; set; }
        public Usuario? UsuarioCriador { get; set; }

        [Column("data_criacao")]
        public DateTime? DataCriacao { get; set; }

        [Column("aprovado")]
        public bool Aprovado { get; set; }

        // Note: removed incorrect collection navigations. A BaseConhecimento
        // references a single Categoria and a single UsuarioCriador; it does not
        // own collections of Usuario or Categoria.
    }
}