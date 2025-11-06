using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class AvaliacaoController : Controller
 {
 private readonly string _connectionString;

 public AvaliacaoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<Avaliacao>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT avaliacao_id, chamado_id, usuario_solicitante_id, nota, comentario, data_avaliacao FROM public.avaliacao ORDER BY avaliacao_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Avaliacao
 {
 AvaliacaoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 UsuarioSolicitanteId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
 Nota = reader.IsDBNull(3) ?0 : reader.GetInt32(3),
 Comentario = reader.IsDBNull(4) ? null : reader.GetString(4),
 DataAvaliacao = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT avaliacao_id, chamado_id, usuario_solicitante_id, nota, comentario, data_avaliacao FROM public.avaliacao WHERE avaliacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Avaliacao
 {
 AvaliacaoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 UsuarioSolicitanteId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
 Nota = reader.IsDBNull(3) ?0 : reader.GetInt32(3),
 Comentario = reader.IsDBNull(4) ? null : reader.GetString(4),
 DataAvaliacao = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new Avaliacao());
 }

 [HttpPost]
 public IActionResult Create(Avaliacao model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.avaliacao (chamado_id, usuario_solicitante_id, nota, comentario, data_avaliacao) VALUES (@chamado_id, @usuario_solicitante_id, @nota, @comentario, @data_avaliacao)", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@usuario_solicitante_id", (object?)model.UsuarioSolicitanteId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@nota", model.Nota);
 cmd.Parameters.AddWithValue("@comentario", (object?)model.Comentario ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_avaliacao", (object?)model.DataAvaliacao ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT avaliacao_id, chamado_id, usuario_solicitante_id, nota, comentario, data_avaliacao FROM public.avaliacao WHERE avaliacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Avaliacao
 {
 AvaliacaoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 UsuarioSolicitanteId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
 Nota = reader.IsDBNull(3) ?0 : reader.GetInt32(3),
 Comentario = reader.IsDBNull(4) ? null : reader.GetString(4),
 DataAvaliacao = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Avaliacao model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.avaliacao SET chamado_id = @chamado_id, usuario_solicitante_id = @usuario_solicitante_id, nota = @nota, comentario = @comentario, data_avaliacao = @data_avaliacao WHERE avaliacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@usuario_solicitante_id", (object?)model.UsuarioSolicitanteId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@nota", model.Nota);
 cmd.Parameters.AddWithValue("@comentario", (object?)model.Comentario ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_avaliacao", (object?)model.DataAvaliacao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.AvaliacaoId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT avaliacao_id, nota, comentario FROM public.avaliacao WHERE avaliacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Avaliacao
 {
 AvaliacaoId = reader.GetInt32(0),
 Nota = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 Comentario = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.avaliacao WHERE avaliacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}