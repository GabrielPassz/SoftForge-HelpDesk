using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class PrioridadeController : Controller
 {
 private readonly string _connectionString;

 public PrioridadeController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<Prioridade>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT prioridade_id, nome_prioridade, descricao FROM public.prioridade ORDER BY prioridade_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Prioridade
 {
 PrioridadeId = reader.GetInt32(0),
 NomePrioridade = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT prioridade_id, nome_prioridade, descricao FROM public.prioridade WHERE prioridade_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Prioridade
 {
 PrioridadeId = reader.GetInt32(0),
 NomePrioridade = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new Prioridade());
 }

 [HttpPost]
 public IActionResult Create(Prioridade model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.prioridade (nome_prioridade, descricao) VALUES (@nome, @descricao)", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomePrioridade ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT prioridade_id, nome_prioridade, descricao FROM public.prioridade WHERE prioridade_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Prioridade
 {
 PrioridadeId = reader.GetInt32(0),
 NomePrioridade = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Prioridade model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.prioridade SET nome_prioridade = @nome, descricao = @descricao WHERE prioridade_id = @id", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomePrioridade ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.PrioridadeId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT prioridade_id, nome_prioridade, descricao FROM public.prioridade WHERE prioridade_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Prioridade
 {
 PrioridadeId = reader.GetInt32(0),
 NomePrioridade = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.prioridade WHERE prioridade_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}