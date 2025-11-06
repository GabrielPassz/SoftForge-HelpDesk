using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class ChamadoController : Controller
 {
 private readonly string _connectionString;

 public ChamadoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<Chamado>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT chamado_id, protocolo, titulo, descricao, data_abertura, data_fechamento, usuario_solicitante_id, tecnico_responsavel_id, categoria_id, prioridade_id, status_id, sla_id, sla_atingido FROM public.chamado ORDER BY chamado_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Chamado
 {
 ChamadoId = reader.GetInt32(0),
 Protocolo = reader.IsDBNull(1) ? null : reader.GetString(1),
 Titulo = reader.IsDBNull(2) ? null : reader.GetString(2),
 Descricao = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataAbertura = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 DataFechamento = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
 UsuarioSolicitanteId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
 TecnicoResponsavelId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
 CategoriaId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
 PrioridadeId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
 StatusId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
 SlaId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
 SlaAtingido = !reader.IsDBNull(12) && reader.GetBoolean(12)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT chamado_id, protocolo, titulo, descricao, data_abertura, data_fechamento, usuario_solicitante_id, tecnico_responsavel_id, categoria_id, prioridade_id, status_id, sla_id, sla_atingido FROM public.chamado WHERE chamado_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Chamado
 {
 ChamadoId = reader.GetInt32(0),
 Protocolo = reader.IsDBNull(1) ? null : reader.GetString(1),
 Titulo = reader.IsDBNull(2) ? null : reader.GetString(2),
 Descricao = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataAbertura = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 DataFechamento = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
 UsuarioSolicitanteId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
 TecnicoResponsavelId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
 CategoriaId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
 PrioridadeId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
 StatusId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
 SlaId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
 SlaAtingido = !reader.IsDBNull(12) && reader.GetBoolean(12)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new Chamado());
 }

 [HttpPost]
 public IActionResult Create(Chamado model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand(@"INSERT INTO public.chamado (protocolo, titulo, descricao, data_abertura, data_fechamento, usuario_solicitante_id, tecnico_responsavel_id, categoria_id, prioridade_id, status_id, sla_id, sla_atingido)
VALUES (@protocolo, @titulo, @descricao, @data_abertura, @data_fechamento, @usuario_solicitante_id, @tecnico_responsavel_id, @categoria_id, @prioridade_id, @status_id, @sla_id, @sla_atingido)", conn);
 cmd.Parameters.AddWithValue("@protocolo", (object?)model.Protocolo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@titulo", (object?)model.Titulo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_abertura", (object?)model.DataAbertura ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_fechamento", (object?)model.DataFechamento ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@usuario_solicitante_id", (object?)model.UsuarioSolicitanteId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@tecnico_responsavel_id", (object?)model.TecnicoResponsavelId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@categoria_id", (object?)model.CategoriaId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@prioridade_id", (object?)model.PrioridadeId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@status_id", (object?)model.StatusId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@sla_id", (object?)model.SlaId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@sla_atingido", model.SlaAtingido);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT chamado_id, protocolo, titulo, descricao, data_abertura, data_fechamento, usuario_solicitante_id, tecnico_responsavel_id, categoria_id, prioridade_id, status_id, sla_id, sla_atingido FROM public.chamado WHERE chamado_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Chamado
 {
 ChamadoId = reader.GetInt32(0),
 Protocolo = reader.IsDBNull(1) ? null : reader.GetString(1),
 Titulo = reader.IsDBNull(2) ? null : reader.GetString(2),
 Descricao = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataAbertura = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 DataFechamento = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
 UsuarioSolicitanteId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
 TecnicoResponsavelId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
 CategoriaId = reader.IsDBNull(8) ? null : reader.GetInt32(8),
 PrioridadeId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
 StatusId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
 SlaId = reader.IsDBNull(11) ? null : reader.GetInt32(11),
 SlaAtingido = !reader.IsDBNull(12) && reader.GetBoolean(12)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Chamado model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand(@"UPDATE public.chamado SET protocolo = @protocolo, titulo = @titulo, descricao = @descricao, data_abertura = @data_abertura, data_fechamento = @data_fechamento, usuario_solicitante_id = @usuario_solicitante_id, tecnico_responsavel_id = @tecnico_responsavel_id, categoria_id = @categoria_id, prioridade_id = @prioridade_id, status_id = @status_id, sla_id = @sla_id, sla_atingido = @sla_atingido WHERE chamado_id = @id", conn);
 cmd.Parameters.AddWithValue("@protocolo", (object?)model.Protocolo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@titulo", (object?)model.Titulo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_abertura", (object?)model.DataAbertura ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_fechamento", (object?)model.DataFechamento ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@usuario_solicitante_id", (object?)model.UsuarioSolicitanteId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@tecnico_responsavel_id", (object?)model.TecnicoResponsavelId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@categoria_id", (object?)model.CategoriaId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@prioridade_id", (object?)model.PrioridadeId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@status_id", (object?)model.StatusId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@sla_id", (object?)model.SlaId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@sla_atingido", model.SlaAtingido);
 cmd.Parameters.AddWithValue("@id", model.ChamadoId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT chamado_id, protocolo, titulo, descricao FROM public.chamado WHERE chamado_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Chamado
 {
 ChamadoId = reader.GetInt32(0),
 Protocolo = reader.IsDBNull(1) ? null : reader.GetString(1),
 Titulo = reader.IsDBNull(2) ? null : reader.GetString(2),
 Descricao = reader.IsDBNull(3) ? null : reader.GetString(3)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.chamado WHERE chamado_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}
