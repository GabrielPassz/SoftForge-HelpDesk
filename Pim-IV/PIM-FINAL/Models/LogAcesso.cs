using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_FINAL.Models
{
    public class LogAcesso
    {
        [Key]
        [Column("log_id")]
        public int LogId { get; set; }

        [ForeignKey("Usuario")]
        [Column("usuario_id")]
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Column("acao")]
        public string? Acao { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("data_hora")]
        public DateTime? DataHora { get; set; }

        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("dispositivo")]
        public string? Dispositivo { get; set; }
    }
}