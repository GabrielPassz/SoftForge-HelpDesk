using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PIM_FINAL.Controllers
{
 public class SlaController : Controller
 {
 private readonly string _connectionString;
 public SlaController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }
 private NpgsqlConnection OpenConn(){ var c=new NpgsqlConnection(_connectionString); c.Open(); return c; }
 private void LoadPrioridades(){ using var conn = OpenConn(); var list = new List<SelectListItem>(); using (var cmd = new NpgsqlCommand("SELECT prioridade_id, nome_prioridade FROM public.prioridade ORDER BY prioridade_id", conn)) using (var r = cmd.ExecuteReader()){ while(r.Read()){ var id=r.GetInt32(0); var nome = r.IsDBNull(1)?("Prioridade #"+id): r.GetString(1); list.Add(new SelectListItem(nome, id.ToString())); } } ViewBag.Prioridades = list; }

 public IActionResult Index()
 {
 var list = new List<Sla>();
 using var conn = OpenConn();
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
 using var conn = OpenConn();
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
 LoadPrioridades();
 return View(new Sla());
 }

 [HttpPost]
 public IActionResult Create(Sla model)
 {
 if (!ModelState.IsValid){ LoadPrioridades(); return View(model); }
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand("INSERT INTO public.sla (nome_sla, prioridade_id, tempo_primeira_resposta, tempo_maximo_resolucao, descricao) VALUES (@nome, @prioridade_id, @tempo_primeira_resposta, @tempo_maximo_resolucao, @descricao)", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomeSla ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@prioridade_id", model.PrioridadeId ?? (object)System.DBNull.Value);
 cmd.Parameters.AddWithValue("@tempo_primeira_resposta", model.TempoPrimeiraResposta);
 cmd.Parameters.AddWithValue("@tempo_maximo_resolucao", model.TempoMaximoResolucao);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = OpenConn();
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
 LoadPrioridades();
 ViewBag.SelectedPrioridade = model.PrioridadeId.ToString();
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Sla model)
 {
 if (!ModelState.IsValid){ LoadPrioridades(); return View(model); }
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand("UPDATE public.sla SET nome_sla = @nome, prioridade_id = @prioridade_id, tempo_primeira_resposta = @tempo_primeira_resposta, tempo_maximo_resolucao = @tempo_maximo_resolucao, descricao = @descricao WHERE sla_id = @id", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomeSla ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@prioridade_id", model.PrioridadeId ?? (object)System.DBNull.Value);
 cmd.Parameters.AddWithValue("@tempo_primeira_resposta", model.TempoPrimeiraResposta);
 cmd.Parameters.AddWithValue("@tempo_maximo_resolucao", model.TempoMaximoResolucao);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.SlaId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = OpenConn();
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
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand("DELETE FROM public.sla WHERE sla_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}