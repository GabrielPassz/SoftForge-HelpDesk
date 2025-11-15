using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PIM_FINAL.Models
{
 public class EditUsuarioViewModel
 {
 public int UsuarioId { get; set; }

 [Required]
 [Display(Name = "Nome completo")]
 public string NomeCompleto { get; set; }

 [Required]
 [EmailAddress]
 public string Email { get; set; }

 [Display(Name = "Perfil")]
 public int? PerfilId { get; set; }

 [Display(Name = "Departamento")]
 public int? DepartamentoId { get; set; }

 public List<SelectListItem> Perfis { get; set; } = new List<SelectListItem>();
 public List<SelectListItem> Departamentos { get; set; } = new List<SelectListItem>();

 // optional: admin can reset password (plain text input -> hashed)
 [DataType(DataType.Password)]
 [Display(Name = "Nova senha (opcional)")]
 public string? NewPassword { get; set; }
 }
}