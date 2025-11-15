using System;using System.Collections.Generic;
namespace PIM_FINAL.Models
{
 public class RelatoriosViewModel
 {
 public DateTime PeriodoInicio { get; set; }
 public DateTime PeriodoFim { get; set; }
 public int TotalChamadosPeriodo { get; set; }
 public int TotalResolvidosPeriodo { get; set; }
 public double TaxaResolucaoPercent { get; set; }
 public string TempoMedioResolucao { get; set; } = "-";
 public int TotalAvaliacoes { get; set; }
 public double SatisfacaoMedia { get; set; }
 public List<ItemResumo> Status { get; set; } = new();
 public List<ItemResumo> Prioridades { get; set; } = new();
 public List<ItemResumo> Categorias { get; set; } = new();
 public List<ItemResumo> Departamentos { get; set; } = new();
 public List<TecnicoResumo> Tecnicos { get; set; } = new();
 public List<SlaResumo> SlaPorPrioridade { get; set; } = new();
 public List<ItemResumo> DistribuicaoNotas { get; set; } = new();
 }
 public class ItemResumo { public string Nome { get; set; } = string.Empty; public int Quantidade { get; set; } public double Percentual { get; set; } public string Extra { get; set; } = string.Empty; }
 public class TecnicoResumo { public string Nome { get; set; } = string.Empty; public int Atendidos { get; set; } public int Resolvidos { get; set; } public double TaxaResolucao { get; set; } public string TempoMedio { get; set; } = "-"; public double AvaliacaoMedia { get; set; } }
 public class SlaResumo { public string Prioridade { get; set; } = string.Empty; public int Total { get; set; } public int Cumpridos { get; set; } public int Violados { get; set; } public double TaxaCumprimento { get; set; } }
}