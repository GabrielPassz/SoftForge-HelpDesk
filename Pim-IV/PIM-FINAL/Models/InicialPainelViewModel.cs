using System.Collections.Generic;

namespace PIM_FINAL.Models
{
 public class InicialPainelViewModel
 {
 public string? NomeUsuario { get; set; }
 public string? Perfil { get; set; }
 public List<string>? RecentNotifications { get; set; }
 public int ChamadosEmEspera { get; set; }
 public int ChamadosEmAtendimento { get; set; }
 public int ResolvidosHoje { get; set; }
 public int SLACumpridoPercent { get; set; }
 }
}