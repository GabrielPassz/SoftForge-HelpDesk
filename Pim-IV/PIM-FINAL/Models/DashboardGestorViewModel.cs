using System.Collections.Generic;

namespace PIM_FINAL.Models
{
 public class DashboardGestorViewModel
 {
 public int TotalChamados { get; set; }
 public int ChamadosResolvidos { get; set; }
 public double TaxaResolucaoPercent { get; set; }
 public int SLACumpridoPercent { get; set; }
 public string TempoMedioResolucao { get; set; } = "-";
 public double SatisfacaoMedia { get; set; }
 public int TotalAvaliacoes { get; set; }
 public List<(string Tecnico, int Atendidos, int Resolvidos, double Taxa, string TempoMedio, double Avaliacao)> DesempenhoEquipe { get; set; } = new List<(string,int,int,double,string,double)>();
 }
}