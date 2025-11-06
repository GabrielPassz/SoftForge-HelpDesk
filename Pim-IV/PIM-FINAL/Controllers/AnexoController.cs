using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class AnexoController : Controller
 {
 private readonly string _connectionString;

 public AnexoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<Anexo>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT anexo_id, chamado_id, nome_arquivo, tipo_arquivo, caminho_arquivo, data_upload, usuario_id FROM public.anexo ORDER BY anexo_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Anexo
 {
 AnexoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 NomeArquivo = reader.IsDBNull(2) ? null : reader.GetString(2),
 TipoArquivo = reader.IsDBNull(3) ? null : reader.GetString(3),
 CaminhoArquivo = reader.IsDBNull(4) ? null : reader.GetString(4),
 DataUpload = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
 UsuarioId = reader.IsDBNull(6) ? null : reader.GetInt32(6)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT anexo_id, chamado_id, nome_arquivo, tipo_arquivo, caminho_arquivo, data_upload, usuario_id FROM public.anexo WHERE anexo_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Anexo
 {
 AnexoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 NomeArquivo = reader.IsDBNull(2) ? null : reader.GetString(2),
 TipoArquivo = reader.IsDBNull(3) ? null : reader.GetString(3),
 CaminhoArquivo = reader.IsDBNull(4) ? null : reader.GetString(4),
 DataUpload = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
 UsuarioId = reader.IsDBNull(6) ? null : reader.GetInt32(6)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new Anexo());
 }

 [HttpPost]
 public IActionResult Create(Anexo model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.anexo (chamado_id, nome_arquivo, tipo_arquivo, caminho_arquivo, data_upload, usuario_id) VALUES (@chamado_id, @nome_arquivo, @tipo_arquivo, @caminho_arquivo, @data_upload, @usuario_id)", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@nome_arquivo", (object?)model.NomeArquivo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@tipo_arquivo", (object?)model.TipoArquivo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@caminho_arquivo", (object?)model.CaminhoArquivo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_upload", (object?)model.DataUpload ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@usuario_id", (object?)model.UsuarioId ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT anexo_id, chamado_id, nome_arquivo, tipo_arquivo, caminho_arquivo, data_upload, usuario_id FROM public.anexo WHERE anexo_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Anexo
 {
 AnexoId = reader.GetInt32(0),
 ChamadoId = reader.IsDBNull(1) ?0 : reader.GetInt32(1),
 NomeArquivo = reader.IsDBNull(2) ? null : reader.GetString(2),
 TipoArquivo = reader.IsDBNull(3) ? null : reader.GetString(3),
 CaminhoArquivo = reader.IsDBNull(4) ? null : reader.GetString(4),
 DataUpload = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
 UsuarioId = reader.IsDBNull(6) ? null : reader.GetInt32(6)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Anexo model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.anexo SET chamado_id = @chamado_id, nome_arquivo = @nome_arquivo, tipo_arquivo = @tipo_arquivo, caminho_arquivo = @caminho_arquivo, data_upload = @data_upload, usuario_id = @usuario_id WHERE anexo_id = @id", conn);
 cmd.Parameters.AddWithValue("@chamado_id", model.ChamadoId);
 cmd.Parameters.AddWithValue("@nome_arquivo", (object?)model.NomeArquivo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@tipo_arquivo", (object?)model.TipoArquivo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@caminho_arquivo", (object?)model.CaminhoArquivo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_upload", (object?)model.DataUpload ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@usuario_id", (object?)model.UsuarioId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.AnexoId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT anexo_id, nome_arquivo, caminho_arquivo FROM public.anexo WHERE anexo_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Anexo
 {
 AnexoId = reader.GetInt32(0),
 NomeArquivo = reader.IsDBNull(1) ? null : reader.GetString(1),
 CaminhoArquivo = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.anexo WHERE anexo_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}