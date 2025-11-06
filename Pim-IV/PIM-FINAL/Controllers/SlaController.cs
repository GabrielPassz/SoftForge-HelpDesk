using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class SlaController : Controller
 {
 private readonly string _connectionString;

 public SlaController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<Sla>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT sla_id, nome_sla, prioridade_id, tempo_primeira_resposta, tempo_maximo_resolucao, descricao FROM public.sla ORDER BY sla_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Sla
 {
 SlaId = reader.GetInt32(0),
 NomeSla = reader.IsDBNull(1) ? null : reader.GetString(1),
 PrioridadeId = reader.IsDBNull(2) ?0 : reader.GetInt32(2),
 TempoPrimeiraResposta = reader.IsDBNull(3) ?0 : reader.GetInt32(3),
 TempoMaximoResolucao = reader.IsDBNull(4) ?0 : reader.GetInt32(4),
 Descricao = reader.IsDBNull(5) ? null : reader.GetString(5)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT sla_id, nome_sla, prioridade_id, tempo_primeira_resposta, tempo_maximo_resolucao, descricao FROM public.sla WHERE sla_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Sla
 {
 SlaId = reader.GetInt32(0),
 NomeSla = reader.IsDBNull(1) ? null : reader.GetString(1),
 PrioridadeId = reader.IsDBNull(2) ?0 : reader.GetInt32(2),
 TempoPrimeiraResposta = reader.IsDBNull(3) ?0 : reader.GetInt32(3),
 TempoMaximoResolucao = reader.IsDBNull(4) ?0 : reader.GetInt32(4),
 Descricao = reader.IsDBNull(5) ? null : reader.GetString(5)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new Sla());
 }

 [HttpPost]
 public IActionResult Create(Sla model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.sla (nome_sla, prioridade_id, tempo_primeira_resposta, tempo_maximo_resolucao, descricao) VALUES (@nome, @prioridade_id, @tempo_primeira_resposta, @tempo_maximo_resolucao, @descricao)", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomeSla ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@prioridade_id", model.PrioridadeId);
 cmd.Parameters.AddWithValue("@tempo_primeira_resposta", model.TempoPrimeiraResposta);
 cmd.Parameters.AddWithValue("@tempo_maximo_resolucao", model.TempoMaximoResolucao);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT sla_id, nome_sla, prioridade_id, tempo_primeira_resposta, tempo_maximo_resolucao, descricao FROM public.sla WHERE sla_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Sla
 {
 SlaId = reader.GetInt32(0),
 NomeSla = reader.IsDBNull(1) ? null : reader.GetString(1),
 PrioridadeId = reader.IsDBNull(2) ?0 : reader.GetInt32(2),
 TempoPrimeiraResposta = reader.IsDBNull(3) ?0 : reader.GetInt32(3),
 TempoMaximoResolucao = reader.IsDBNull(4) ?0 : reader.GetInt32(4),
 Descricao = reader.IsDBNull(5) ? null : reader.GetString(5)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Sla model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.sla SET nome_sla = @nome, prioridade_id = @prioridade_id, tempo_primeira_resposta = @tempo_primeira_resposta, tempo_maximo_resolucao = @tempo_maximo_resolucao, descricao = @descricao WHERE sla_id = @id", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomeSla ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@prioridade_id", model.PrioridadeId);
 cmd.Parameters.AddWithValue("@tempo_primeira_resposta", model.TempoPrimeiraResposta);
 cmd.Parameters.AddWithValue("@tempo_maximo_resolucao", model.TempoMaximoResolucao);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.SlaId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT sla_id, nome_sla FROM public.sla WHERE sla_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Sla
 {
 SlaId = reader.GetInt32(0),
 NomeSla = reader.IsDBNull(1) ? null : reader.GetString(1)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.sla WHERE sla_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}