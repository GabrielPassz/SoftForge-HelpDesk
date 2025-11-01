using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM.Models
{
    [Table("atendimento")]
    public class Atendimento
    {
        [Key]
        [Column("atendimento_id")]
        public int AtendimentoId { get; set; }

        [ForeignKey("Chamado")]
        [Column("chamado_id")]
        public int ChamadoId { get; set; }
        public Chamado Chamado { get; set; }

        [ForeignKey("UsuarioTecnico")]
        [Column("usuario_tecnico_id")]
        public int UsuarioTecnicoId { get; set; }
        public Usuario UsuarioTecnico { get; set; }

        [Column("data_atendimento")]
        public DateTime DataAtendimento { get; set; } = DateTime.Now;

        [Required]
        [Column("acao_realizada")]
        public string AcaoRealizada { get; set; }

        [Required]
        [Column("tempo_gasto")]
        public int TempoGasto { get; set; }

        [Column("solucao_ia")]
        public bool SolucaoIA { get; set; }

        [ForeignKey("BaseConhecimento")]
        [Column("solucao_base_conhecimento_id")]
        public int? SolucaoBaseConhecimentoId { get; set; }
        public BaseConhecimento SolucaoBaseConhecimento { get; set; }
    }

}
