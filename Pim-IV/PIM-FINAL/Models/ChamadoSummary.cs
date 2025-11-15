using System;

namespace PIM_FINAL.Models
{
 public class ChamadoSummary
 {
 public int ChamadoId { get; set; }
 public string? Protocolo { get; set; }
 public string? Titulo { get; set; }
 public string? Prioridade { get; set; }
 public string? Tecnico { get; set; }
 public int SlaConsumedPercent { get; set; }
 public TimeSpan? TimeRemaining { get; set; }
 }
}