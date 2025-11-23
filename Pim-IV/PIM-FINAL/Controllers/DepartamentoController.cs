using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PIM_FINAL.Controllers
{
 public class DepartamentoController : Controller
 {
 private readonly string _connectionString;
 // Permite letras, números, espaços e pontuação básica (vírgula, ponto, hífen, parênteses) até250 caracteres
 private static readonly Regex SafeText = new Regex("^[\\p{L}0-9 ,;:_\\-\\.\\(\\)'\"/]{0,250}$", RegexOptions.Compiled);
 public DepartamentoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }
 private bool IsUnsafe(string? v) => !string.IsNullOrWhiteSpace(v) && !SafeText.IsMatch(v);
 private bool ConnInvalid() => string.IsNullOrWhiteSpace(_connectionString);

 public IActionResult Index()
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return View(new List<Departamento>()); }
 try
 {
 var list = new List<Departamento>();
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("SELECT departamento_id, nome_departamento, descricao FROM public.departamento ORDER BY departamento_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Departamento
 {
 DepartamentoId = reader.GetInt32(0),
 NomeDepartamento = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 });
 }
 return View(list);
 }
 catch { TempData["ErrorMessage"] = "Falha ao carregar departamentos."; return View(new List<Departamento>()); }
 }

 public IActionResult Details(int id)
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return RedirectToAction(nameof(Index)); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("SELECT departamento_id, nome_departamento, descricao FROM public.departamento WHERE departamento_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Departamento
 {
 DepartamentoId = reader.GetInt32(0),
 NomeDepartamento = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }
 catch { TempData["ErrorMessage"] = "Erro ao carregar detalhes."; return RedirectToAction(nameof(Index)); }
 }

 public IActionResult Create() => View(new Departamento());

 [HttpPost]
 public IActionResult Create(Departamento model)
 {
 if (IsUnsafe(model.NomeDepartamento) || IsUnsafe(model.Descricao)) ModelState.AddModelError(string.Empty, "Texto inválido (caracteres não permitidos ou muito longo)." );
 if (!ModelState.IsValid) return View(model);
 if (ConnInvalid()) { ModelState.AddModelError(string.Empty, "Conexão não configurada."); return View(model); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.departamento (nome_departamento, descricao) VALUES (@nome, @descricao)", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomeDepartamento ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 var rows = cmd.ExecuteNonQuery();
 if (rows ==0) { TempData["ErrorMessage"] = "Nenhuma linha inserida."; return View(model); }
 TempData["SuccessMessage"] = "Departamento criado.";
 return RedirectToAction(nameof(Index));
 }
 catch { TempData["ErrorMessage"] = "Erro ao criar departamento."; return View(model); }
 }

 public IActionResult Edit(int id)
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return RedirectToAction(nameof(Index)); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("SELECT departamento_id, nome_departamento, descricao FROM public.departamento WHERE departamento_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Departamento
 {
 DepartamentoId = reader.GetInt32(0),
 NomeDepartamento = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }
 catch { TempData["ErrorMessage"] = "Erro ao carregar departamento para edição."; return RedirectToAction(nameof(Index)); }
 }

 [HttpPost]
 public IActionResult Edit(Departamento model)
 {
 if (IsUnsafe(model.NomeDepartamento) || IsUnsafe(model.Descricao)) ModelState.AddModelError(string.Empty, "Texto inválido (caracteres não permitidos ou muito longo)." );
 if (!ModelState.IsValid) return View(model);
 if (ConnInvalid()) { ModelState.AddModelError(string.Empty, "Conexão não configurada."); return View(model); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.departamento SET nome_departamento = @nome, descricao = @descricao WHERE departamento_id = @id", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomeDepartamento ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.DepartamentoId);
 var rows = cmd.ExecuteNonQuery();
 if (rows ==0)
 {
 TempData["ErrorMessage"] = "Nenhuma linha atualizada (ID inexistente?).";
 return View(model);
 }
 TempData["SuccessMessage"] = "Departamento atualizado.";
 return RedirectToAction(nameof(Index));
 }
 catch { TempData["ErrorMessage"] = "Erro ao atualizar departamento."; return View(model); }
 }

 public IActionResult Delete(int id)
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return RedirectToAction(nameof(Index)); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("SELECT departamento_id, nome_departamento, descricao FROM public.departamento WHERE departamento_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Departamento
 {
 DepartamentoId = reader.GetInt32(0),
 NomeDepartamento = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }
 catch { TempData["ErrorMessage"] = "Erro ao carregar departamento para exclusão."; return RedirectToAction(nameof(Index)); }
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 if (ConnInvalid()) { TempData["ErrorMessage"] = "Conexão não configurada."; return RedirectToAction(nameof(Index)); }
 try
 {
 using var conn = new NpgsqlConnection(_connectionString); conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.departamento WHERE departamento_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 var rows = cmd.ExecuteNonQuery();
 TempData["SuccessMessage"] = rows >0 ? "Departamento removido." : "Nenhuma linha removida.";
 }
 catch { TempData["ErrorMessage"] = "Erro ao remover departamento."; }
 return RedirectToAction(nameof(Index));
 }
 }
}
