using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class IaAnaliseController : Controller
 {
 private readonly string _connectionString;

 public IaAnaliseController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<IaAnalise>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT ia_id, chamado_id, categoria_prevista, confianca, resultado_validado, comentario, data_analise FROM public.ia_analise ORDER BY ia_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new IaAnalise
 {
 IaId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 CategoriaPrevista = reader.IsDBNull(2) ? null : reader.GetString(2),
 Confianca = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
 ResultadoValidado = reader.IsDBNull(4) ? null : reader.GetBoolean(4),
 Comentario = reader.IsDBNull(5) ? null : reader.GetString(5),
 DataAnalise = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT ia_id, chamado_id, categoria_prevista, confianca, resultado_validado, comentario, data_analise FROM public.ia_analise WHERE ia_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new IaAnalise
 {
 IaId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 CategoriaPrevista = reader.IsDBNull(2) ? null : reader.GetString(2),
 Confianca = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
 ResultadoValidado = reader.IsDBNull(4) ? null : reader.GetBoolean(4),
 Comentario = reader.IsDBNull(5) ? null : reader.GetString(5),
 DataAnalise = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new IaAnalise());
 }

 [HttpPost]
 public IActionResult Create(IaAnalise model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.ia_analise (chamado_id, categoria_prevista, confianca, resultado_validado, comentario, data_analise) VALUES (@chamado_id, @categoria_prevista, @confianca, @resultado_validado, @comentario, @data_analise)", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@categoria_prevista", (object?)model.CategoriaPrevista ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@confianca", (object?)model.Confianca ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@resultado_validado", (object?)model.ResultadoValidado ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@comentario", (object?)model.Comentario ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_analise", (object?)model.DataAnalise ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT ia_id, chamado_id, categoria_prevista, confianca, resultado_validado, comentario, data_analise FROM public.ia_analise WHERE ia_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new IaAnalise
 {
 IaId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 CategoriaPrevista = reader.IsDBNull(2) ? null : reader.GetString(2),
 Confianca = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
 ResultadoValidado = reader.IsDBNull(4) ? null : reader.GetBoolean(4),
 Comentario = reader.IsDBNull(5) ? null : reader.GetString(5),
 DataAnalise = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(IaAnalise model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.ia_analise SET chamado_id = @chamado_id, categoria_prevista = @categoria_prevista, confianca = @confianca, resultado_validado = @resultado_validado, comentario = @comentario, data_analise = @data_analise WHERE ia_id = @id", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@categoria_prevista", (object?)model.CategoriaPrevista ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@confianca", (object?)model.Confianca ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@resultado_validado", (object?)model.ResultadoValidado ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@comentario", (object?)model.Comentario ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_analise", (object?)model.DataAnalise ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.IaId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT ia_id, categoria_prevista, confianca FROM public.ia_analise WHERE ia_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new IaAnalise
 {
 IaId = reader.GetInt32(0),
 CategoriaPrevista = reader.IsDBNull(1) ? null : reader.GetString(1),
 Confianca = reader.IsDBNull(2) ? null : reader.GetDecimal(2)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.ia_analise WHERE ia_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}