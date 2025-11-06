namespace PIM_FINAL.Models
{
    public class AbrirChamado
    {
        public string SolicitanteNome { get; set; }
        public string SolicitanteDepartamento { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public string Prioridade { get; set; }
        public bool Created { get; set; }
        public string CreatedProtocol { get; set; }
    }
}