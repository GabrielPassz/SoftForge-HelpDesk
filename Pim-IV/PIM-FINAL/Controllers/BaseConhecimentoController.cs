using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class BaseConhecimentoController : Controller
 {
 private readonly string _connectionString;

 public BaseConhecimentoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<BaseConhecimento>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT base_id, titulo, descricao, solucao, categoria_id, usuario_criador_id, data_criacao, aprovado FROM public.base_conhecimento ORDER BY base_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new BaseConhecimento
 {
 BaseId = reader.GetInt32(0),
 Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2),
 Solucao = reader.IsDBNull(3) ? null : reader.GetString(3),
 CategoriaId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
 UsuarioCriadorId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
 DataCriacao = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
 Aprovado = !reader.IsDBNull(7) && reader.GetBoolean(7)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT base_id, titulo, descricao, solucao, categoria_id, usuario_criador_id, data_criacao, aprovado FROM public.base_conhecimento WHERE base_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new BaseConhecimento
 {
 BaseId = reader.GetInt32(0),
 Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2),
 Solucao = reader.IsDBNull(3) ? null : reader.GetString(3),
 CategoriaId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
 UsuarioCriadorId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
 DataCriacao = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
 Aprovado = !reader.IsDBNull(7) && reader.GetBoolean(7)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new BaseConhecimento());
 }

 [HttpPost]
 public IActionResult Create(BaseConhecimento model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand(@"INSERT INTO public.base_conhecimento (titulo, descricao, solucao, categoria_id, usuario_criador_id, data_criacao, aprovado) VALUES (@titulo, @descricao, @solucao, @categoria_id, @usuario_criador_id, @data_criacao, @aprovado)", conn);
 cmd.Parameters.AddWithValue("@titulo", (object?)model.Titulo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@solucao", (object?)model.Solucao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@categoria_id", (object?)model.CategoriaId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@usuario_criador_id", (object?)model.UsuarioCriadorId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_criacao", (object?)model.DataCriacao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@aprovado", model.Aprovado);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT base_id, titulo, descricao, solucao, categoria_id, usuario_criador_id, data_criacao, aprovado FROM public.base_conhecimento WHERE base_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new BaseConhecimento
 {
 BaseId = reader.GetInt32(0),
 Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2),
 Solucao = reader.IsDBNull(3) ? null : reader.GetString(3),
 CategoriaId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
 UsuarioCriadorId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
 DataCriacao = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
 Aprovado = !reader.IsDBNull(7) && reader.GetBoolean(7)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(BaseConhecimento model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand(@"UPDATE public.base_conhecimento SET titulo = @titulo, descricao = @descricao, solucao = @solucao, categoria_id = @categoria_id, usuario_criador_id = @usuario_criador_id, data_criacao = @data_criacao, aprovado = @aprovado WHERE base_id = @id", conn);
 cmd.Parameters.AddWithValue("@titulo", (object?)model.Titulo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@solucao", (object?)model.Solucao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@categoria_id", (object?)model.CategoriaId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@usuario_criador_id", (object?)model.UsuarioCriadorId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_criacao", (object?)model.DataCriacao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@aprovado", model.Aprovado);
 cmd.Parameters.AddWithValue("@id", model.BaseId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT base_id, titulo, descricao FROM public.base_conhecimento WHERE base_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new BaseConhecimento
 {
 BaseId = reader.GetInt32(0),
 Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.base_conhecimento WHERE base_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}