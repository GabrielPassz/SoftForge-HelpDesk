using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class LogAcessoController : Controller
 {
 private readonly string _connectionString;

 public LogAcessoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<LogAcesso>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT log_id, usuario_id, acao, descricao, data_hora, ip_address, dispositivo FROM public.log_acesso ORDER BY log_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new LogAcesso
 {
 LogId = reader.GetInt32(0),
 UsuarioId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
 Acao = reader.IsDBNull(2) ? null : reader.GetString(2),
 Descricao = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataHora = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 IpAddress = reader.IsDBNull(5) ? null : reader.GetString(5),
 Dispositivo = reader.IsDBNull(6) ? null : reader.GetString(6)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT log_id, usuario_id, acao, descricao, data_hora, ip_address, dispositivo FROM public.log_acesso WHERE log_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new LogAcesso
 {
 LogId = reader.GetInt32(0),
 UsuarioId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
 Acao = reader.IsDBNull(2) ? null : reader.GetString(2),
 Descricao = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataHora = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 IpAddress = reader.IsDBNull(5) ? null : reader.GetString(5),
 Dispositivo = reader.IsDBNull(6) ? null : reader.GetString(6)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new LogAcesso());
 }

 [HttpPost]
 public IActionResult Create(LogAcesso model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.log_acesso (usuario_id, acao, descricao, data_hora, ip_address, dispositivo) VALUES (@usuario_id, @acao, @descricao, @data_hora, @ip_address, @dispositivo)", conn);
 cmd.Parameters.AddWithValue("@usuario_id", (object?)model.UsuarioId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@acao", (object?)model.Acao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_hora", (object?)model.DataHora ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@ip_address", (object?)model.IpAddress ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@dispositivo", (object?)model.Dispositivo ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT log_id, usuario_id, acao, descricao, data_hora, ip_address, dispositivo FROM public.log_acesso WHERE log_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new LogAcesso
 {
 LogId = reader.GetInt32(0),
 UsuarioId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
 Acao = reader.IsDBNull(2) ? null : reader.GetString(2),
 Descricao = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataHora = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 IpAddress = reader.IsDBNull(5) ? null : reader.GetString(5),
 Dispositivo = reader.IsDBNull(6) ? null : reader.GetString(6)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(LogAcesso model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.log_acesso SET usuario_id = @usuario_id, acao = @acao, descricao = @descricao, data_hora = @data_hora, ip_address = @ip_address, dispositivo = @dispositivo WHERE log_id = @id", conn);
 cmd.Parameters.AddWithValue("@usuario_id", (object?)model.UsuarioId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@acao", (object?)model.Acao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_hora", (object?)model.DataHora ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@ip_address", (object?)model.IpAddress ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@dispositivo", (object?)model.Dispositivo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.LogId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT log_id, acao, descricao FROM public.log_acesso WHERE log_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new LogAcesso
 {
 LogId = reader.GetInt32(0),
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
 using var cmd = new NpgsqlCommand("DELETE FROM public.log_acesso WHERE log_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}