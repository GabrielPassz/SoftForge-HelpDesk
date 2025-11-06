using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class AtendimentoController : Controller
 {
 private readonly string _connectionString;

 public AtendimentoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<Atendimento>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT atendimento_id, chamado_id, usuario_tecnico_id, data_atendimento, acao_realizada, tempo_gasto, solucao_ia, solucao_base_conhecimento_id FROM public.atendimento ORDER BY atendimento_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Atendimento
 {
 AtendimentoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 UsuarioTecnicoId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
 DataAtendimento = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
 AcaoRealizada = reader.IsDBNull(4) ? null : reader.GetString(4),
 TempoGasto = reader.IsDBNull(5) ?0 : reader.GetInt32(5),
 SolucaoIa = !reader.IsDBNull(6) && reader.GetBoolean(6),
 SolucaoBaseConhecimentoId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT atendimento_id, chamado_id, usuario_tecnico_id, data_atendimento, acao_realizada, tempo_gasto, solucao_ia, solucao_base_conhecimento_id FROM public.atendimento WHERE atendimento_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Atendimento
 {
 AtendimentoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 UsuarioTecnicoId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
 DataAtendimento = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
 AcaoRealizada = reader.IsDBNull(4) ? null : reader.GetString(4),
 TempoGasto = reader.IsDBNull(5) ?0 : reader.GetInt32(5),
 SolucaoIa = !reader.IsDBNull(6) && reader.GetBoolean(6),
 SolucaoBaseConhecimentoId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new Atendimento());
 }

 [HttpPost]
 public IActionResult Create(Atendimento model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand(@"INSERT INTO public.atendimento (chamado_id, usuario_tecnico_id, data_atendimento, acao_realizada, tempo_gasto, solucao_ia, solucao_base_conhecimento_id) VALUES (@chamado_id, @usuario_tecnico_id, @data_atendimento, @acao_realizada, @tempo_gasto, @solucao_ia, @solucao_base_conhecimento_id)", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@usuario_tecnico_id", (object?)model.UsuarioTecnicoId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_atendimento", (object?)model.DataAtendimento ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@acao_realizada", (object?)model.AcaoRealizada ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@tempo_gasto", model.TempoGasto);
 cmd.Parameters.AddWithValue("@solucao_ia", model.SolucaoIa);
 cmd.Parameters.AddWithValue("@solucao_base_conhecimento_id", (object?)model.SolucaoBaseConhecimentoId ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT atendimento_id, chamado_id, usuario_tecnico_id, data_atendimento, acao_realizada, tempo_gasto, solucao_ia, solucao_base_conhecimento_id FROM public.atendimento WHERE atendimento_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Atendimento
 {
 AtendimentoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 UsuarioTecnicoId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
 DataAtendimento = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
 AcaoRealizada = reader.IsDBNull(4) ? null : reader.GetString(4),
 TempoGasto = reader.IsDBNull(5) ?0 : reader.GetInt32(5),
 SolucaoIa = !reader.IsDBNull(6) && reader.GetBoolean(6),
 SolucaoBaseConhecimentoId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Atendimento model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand(@"UPDATE public.atendimento SET chamado_id = @chamado_id, usuario_tecnico_id = @usuario_tecnico_id, data_atendimento = @data_atendimento, acao_realizada = @acao_realizada, tempo_gasto = @tempo_gasto, solucao_ia = @solucao_ia, solucao_base_conhecimento_id = @solucao_base_conhecimento_id WHERE atendimento_id = @id", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@usuario_tecnico_id", (object?)model.UsuarioTecnicoId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_atendimento", (object?)model.DataAtendimento ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@acao_realizada", (object?)model.AcaoRealizada ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@tempo_gasto", model.TempoGasto);
 cmd.Parameters.AddWithValue("@solucao_ia", model.SolucaoIa);
 cmd.Parameters.AddWithValue("@solucao_base_conhecimento_id", (object?)model.SolucaoBaseConhecimentoId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.AtendimentoId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT atendimento_id, acao_realizada FROM public.atendimento WHERE atendimento_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Atendimento
 {
 AtendimentoId = reader.GetInt32(0),
 AcaoRealizada = reader.IsDBNull(1) ? null : reader.GetString(1)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.atendimento WHERE atendimento_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}