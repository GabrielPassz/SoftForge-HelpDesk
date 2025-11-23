using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PIM_FINAL.Controllers
{
 public class PerfilUsuarioController : Controller
 {
 private readonly string _connectionString;
 private readonly ILogger<PerfilUsuarioController> _logger;
 // Permitir até250 chars com vírgulas, ponto, hífen, parênteses, barras e aspas
 private static readonly Regex SafeText = new Regex(@"^[\p{L}0-9 ,;:_\-\.\(\)'""/]{0,250}$", RegexOptions.Compiled);

 public PerfilUsuarioController(IConfiguration config, ILogger<PerfilUsuarioController> logger)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 _logger = logger;
 }

 private bool IsUnsafe(string? v) => !string.IsNullOrWhiteSpace(v) && !SafeText.IsMatch(v);
 private bool ConnInvalid() => string.IsNullOrWhiteSpace(_connectionString);

 public IActionResult Index()
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return View(new List<PerfilUsuario>()); }
 try
 {
 var list = new List<PerfilUsuario>();
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("SELECT perfil_id, nome_perfil, descricao FROM public.perfil_usuario ORDER BY perfil_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new PerfilUsuario
 {
 PerfilId = reader.GetInt32(0),
 NomePerfil = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 });
 }
 return View(list);
 }
 catch (NpgsqlException ex)
 {
 _logger.LogError(ex, "Falha ao carregar perfis");
 TempData["ErrorMessage"] = "Falha ao carregar perfis.";
 TempData["ErrorDetails"] = ex.Message;
 return View(new List<PerfilUsuario>());
 }
 }

 public IActionResult Details(int id)
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return RedirectToAction(nameof(Index)); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("SELECT perfil_id, nome_perfil, descricao FROM public.perfil_usuario WHERE perfil_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new PerfilUsuario
 {
 PerfilId = reader.GetInt32(0),
 NomePerfil = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }
 catch (NpgsqlException ex)
 {
 _logger.LogError(ex, "Erro ao carregar detalhes do perfil {PerfilId}", id);
 TempData["ErrorMessage"] = "Erro ao carregar detalhes do perfil.";
 TempData["ErrorDetails"] = ex.Message;
 return RedirectToAction(nameof(Index));
 }
 }

 public IActionResult Create() => View(new PerfilUsuario());

 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Create(PerfilUsuario model)
 {
 if (IsUnsafe(model.NomePerfil) || IsUnsafe(model.Descricao))
 {
 ModelState.AddModelError(string.Empty, "Texto inválido (caracteres não permitidos ou muito longo)." );
 }
 if (!ModelState.IsValid) return View(model);
 if (ConnInvalid()) { ModelState.AddModelError(string.Empty, "Conexão não configurada." ); return View(model); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.perfil_usuario (nome_perfil, descricao) VALUES (@nome, @descricao)", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomePerfil ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 var rows = cmd.ExecuteNonQuery();
 if (rows ==0) { TempData["ErrorMessage"] = "Nenhuma linha inserida."; return View(model); }
 TempData["SuccessMessage"] = "Perfil criado.";
 return RedirectToAction(nameof(Index));
 }
 catch (NpgsqlException ex)
 {
 _logger.LogError(ex, "Erro ao criar perfil");
 TempData["ErrorMessage"] = "Erro ao criar perfil.";
 TempData["ErrorDetails"] = ex.Message;
 return View(model);
 }
 }

 public IActionResult Edit(int id)
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return RedirectToAction(nameof(Index)); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("SELECT perfil_id, nome_perfil, descricao FROM public.perfil_usuario WHERE perfil_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new PerfilUsuario
 {
 PerfilId = reader.GetInt32(0),
 NomePerfil = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }
 catch (NpgsqlException ex)
 {
 _logger.LogError(ex, "Erro ao carregar perfil para edição {PerfilId}", id);
 TempData["ErrorMessage"] = "Erro ao carregar perfil para edição.";
 TempData["ErrorDetails"] = ex.Message;
 return RedirectToAction(nameof(Index));
 }
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Edit(PerfilUsuario model)
 {
 if (IsUnsafe(model.NomePerfil) || IsUnsafe(model.Descricao)) ModelState.AddModelError(string.Empty, "Texto inválido (caracteres não permitidos ou muito longo)." );
 if (!ModelState.IsValid) return View(model);
 if (ConnInvalid()) { ModelState.AddModelError(string.Empty, "Conexão não configurada." ); return View(model); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.perfil_usuario SET nome_perfil = @nome, descricao = @descricao WHERE perfil_id = @id", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomePerfil ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.PerfilId);
 var rows = cmd.ExecuteNonQuery();
 if (rows ==0)
 {
 TempData["ErrorMessage"] = "Nenhuma linha atualizada (ID inexistente?).";
 return View(model);
 }
 TempData["SuccessMessage"] = "Perfil atualizado.";
 return RedirectToAction(nameof(Index));
 }
 catch (NpgsqlException ex)
 {
 _logger.LogError(ex, "Erro ao atualizar perfil {PerfilId}", model.PerfilId);
 TempData["ErrorMessage"] = "Erro ao atualizar perfil.";
 TempData["ErrorDetails"] = ex.Message;
 return View(model);
 }
 }

 public IActionResult Delete(int id)
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return RedirectToAction(nameof(Index)); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("SELECT perfil_id, nome_perfil, descricao FROM public.perfil_usuario WHERE perfil_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new PerfilUsuario
 {
 PerfilId = reader.GetInt32(0),
 NomePerfil = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }
 catch (NpgsqlException ex)
 {
 _logger.LogError(ex, "Erro ao carregar perfil para exclusão {PerfilId}", id);
 TempData["ErrorMessage"] = "Erro ao carregar perfil para exclusão.";
 TempData["ErrorDetails"] = ex.Message;
 return RedirectToAction(nameof(Index));
 }
 }

 [HttpPost, ActionName("Delete")]
 [ValidateAntiForgeryToken]
 public IActionResult DeleteConfirmed(int id)
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return RedirectToAction(nameof(Index)); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.perfil_usuario WHERE perfil_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 var rows = cmd.ExecuteNonQuery();
 TempData["SuccessMessage"] = rows >0 ? "Perfil removido." : "Nenhuma linha removida.";
 }
 catch (NpgsqlException ex)
 {
 _logger.LogError(ex, "Erro ao remover perfil {PerfilId}", id);
 TempData["ErrorMessage"] = "Erro ao remover perfil.";
 TempData["ErrorDetails"] = ex.Message;
 }
 return RedirectToAction(nameof(Index));
 }
 }
}