using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class ComunicacaoController : Controller
 {
 private readonly string _connectionString;

 public ComunicacaoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<Comunicacao>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT comunicacao_id, chamado_id, usuario_id, mensagem, data_envio FROM public.comunicacao ORDER BY comunicacao_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Comunicacao
 {
 ComunicacaoId = reader.GetInt32(0),
 ChamadoId = reader.GetInt32(1),
 UsuarioId = reader.GetInt32(2),
 Mensagem = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataEnvio = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT comunicacao_id, chamado_id, usuario_id, mensagem, data_envio FROM public.comunicacao WHERE comunicacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Comunicacao
 {
 ComunicacaoId = reader.GetInt32(0),
 ChamadoId = reader.GetInt32(1),
 UsuarioId = reader.GetInt32(2),
 Mensagem = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataEnvio = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new Comunicacao());
 }

 [HttpPost]
 public IActionResult Create(Comunicacao model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.comunicacao (chamado_id, usuario_id, mensagem, data_envio) VALUES (@chamado_id, @usuario_id, @mensagem, @data_envio)", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@usuario_id", model.UsuarioId);
 cmd.Parameters.AddWithValue("@mensagem", (object?)model.Mensagem ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_envio", (object?)model.DataEnvio ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT comunicacao_id, chamado_id, usuario_id, mensagem, data_envio FROM public.comunicacao WHERE comunicacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Comunicacao
 {
 ComunicacaoId = reader.GetInt32(0),
 ChamadoId = reader.GetInt32(1),
 UsuarioId = reader.GetInt32(2),
 Mensagem = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataEnvio = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Comunicacao model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.comunicacao SET chamado_id = @chamado_id, usuario_id = @usuario_id, mensagem = @mensagem, data_envio = @data_envio WHERE comunicacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@usuario_id", model.UsuarioId);
 cmd.Parameters.AddWithValue("@mensagem", (object?)model.Mensagem ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_envio", (object?)model.DataEnvio ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.ComunicacaoId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT comunicacao_id, mensagem FROM public.comunicacao WHERE comunicacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Comunicacao
 {
 ComunicacaoId = reader.GetInt32(0),
 Mensagem = reader.IsDBNull(1) ? null : reader.GetString(1)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.comunicacao WHERE comunicacao_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}