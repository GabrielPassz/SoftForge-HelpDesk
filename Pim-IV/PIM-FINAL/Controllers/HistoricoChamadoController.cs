using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class HistoricoChamadoController : Controller
 {
 private readonly string _connectionString;

 public HistoricoChamadoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<HistoricoChamado>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT historico_id, chamado_id, usuario_id, acao, descricao, data_acao, status_anterior_id, status_novo_id FROM public.historico_chamado ORDER BY historico_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new HistoricoChamado
 {
 HistoricoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 UsuarioId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
 Acao = reader.IsDBNull(3) ? null : reader.GetString(3),
 Descricao = reader.IsDBNull(4) ? null : reader.GetString(4),
 DataAcao = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
 StatusAnteriorId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
 StatusNovoId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT historico_id, chamado_id, usuario_id, acao, descricao, data_acao, status_anterior_id, status_novo_id FROM public.historico_chamado WHERE historico_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new HistoricoChamado
 {
 HistoricoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 UsuarioId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
 Acao = reader.IsDBNull(3) ? null : reader.GetString(3),
 Descricao = reader.IsDBNull(4) ? null : reader.GetString(4),
 DataAcao = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
 StatusAnteriorId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
 StatusNovoId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new HistoricoChamado());
 }

 [HttpPost]
 public IActionResult Create(HistoricoChamado model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand(@"INSERT INTO public.historico_chamado (chamado_id, usuario_id, acao, descricao, data_acao, status_anterior_id, status_novo_id) VALUES (@chamado_id, @usuario_id, @acao, @descricao, @data_acao, @status_anterior_id, @status_novo_id)", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@usuario_id", (object?)model.UsuarioId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@acao", (object?)model.Acao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_acao", (object?)model.DataAcao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@status_anterior_id", (object?)model.StatusAnteriorId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@status_novo_id", (object?)model.StatusNovoId ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT historico_id, chamado_id, usuario_id, acao, descricao, data_acao, status_anterior_id, status_novo_id FROM public.historico_chamado WHERE historico_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new HistoricoChamado
 {
 HistoricoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 UsuarioId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
 Acao = reader.IsDBNull(3) ? null : reader.GetString(3),
 Descricao = reader.IsDBNull(4) ? null : reader.GetString(4),
 DataAcao = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
 StatusAnteriorId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
 StatusNovoId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(HistoricoChamado model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand(@"UPDATE public.historico_chamado SET chamado_id = @chamado_id, usuario_id = @usuario_id, acao = @acao, descricao = @descricao, data_acao = @data_acao, status_anterior_id = @status_anterior_id, status_novo_id = @status_novo_id WHERE historico_id = @id", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@usuario_id", (object?)model.UsuarioId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@acao", (object?)model.Acao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_acao", (object?)model.DataAcao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@status_anterior_id", (object?)model.StatusAnteriorId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@status_novo_id", (object?)model.StatusNovoId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.HistoricoId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT historico_id, acao, descricao FROM public.historico_chamado WHERE historico_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new HistoricoChamado
 {
 HistoricoId = reader.GetInt32(0),
 Acao = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.historico_chamado WHERE historico_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}