using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class FuncionarioController : Controller
 {
 private readonly string _connectionString;

 public FuncionarioController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<Funcionario>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT funcionario_id, usuario_id, nome, email, data_contratacao, cargo, matricula, data_demissao FROM public.funcionario ORDER BY funcionario_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Funcionario
 {
 FuncionarioId = reader.GetInt32(0),
 UsuarioId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
 Nome = reader.IsDBNull(2) ? null : reader.GetString(2),
 Email = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataContratacao = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 Cargo = reader.IsDBNull(5) ? null : reader.GetString(5),
 Matricula = reader.IsDBNull(6) ? null : reader.GetString(6),
 DataDemissao = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT funcionario_id, usuario_id, nome, email, data_contratacao, cargo, matricula, data_demissao FROM public.funcionario WHERE funcionario_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Funcionario
 {
 FuncionarioId = reader.GetInt32(0),
 UsuarioId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
 Nome = reader.IsDBNull(2) ? null : reader.GetString(2),
 Email = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataContratacao = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 Cargo = reader.IsDBNull(5) ? null : reader.GetString(5),
 Matricula = reader.IsDBNull(6) ? null : reader.GetString(6),
 DataDemissao = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new Funcionario());
 }

 [HttpPost]
 public IActionResult Create(Funcionario model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.funcionario (usuario_id, nome, email, data_contratacao, cargo, matricula, data_demissao) VALUES (@usuario_id, @nome, @email, @data_contratacao, @cargo, @matricula, @data_demissao)", conn);
 cmd.Parameters.AddWithValue("@usuario_id", (object?)model.UsuarioId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@nome", (object?)model.Nome ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@email", (object?)model.Email ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_contratacao", (object?)model.DataContratacao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@cargo", (object?)model.Cargo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@matricula", (object?)model.Matricula ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_demissao", (object?)model.DataDemissao ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT funcionario_id, usuario_id, nome, email, data_contratacao, cargo, matricula, data_demissao FROM public.funcionario WHERE funcionario_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Funcionario
 {
 FuncionarioId = reader.GetInt32(0),
 UsuarioId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
 Nome = reader.IsDBNull(2) ? null : reader.GetString(2),
 Email = reader.IsDBNull(3) ? null : reader.GetString(3),
 DataContratacao = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 Cargo = reader.IsDBNull(5) ? null : reader.GetString(5),
 Matricula = reader.IsDBNull(6) ? null : reader.GetString(6),
 DataDemissao = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Funcionario model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.funcionario SET usuario_id = @usuario_id, nome = @nome, email = @email, data_contratacao = @data_contratacao, cargo = @cargo, matricula = @matricula, data_demissao = @data_demissao WHERE funcionario_id = @id", conn);
 cmd.Parameters.AddWithValue("@usuario_id", (object?)model.UsuarioId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@nome", (object?)model.Nome ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@email", (object?)model.Email ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_contratacao", (object?)model.DataContratacao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@cargo", (object?)model.Cargo ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@matricula", (object?)model.Matricula ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_demissao", (object?)model.DataDemissao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.FuncionarioId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT funcionario_id, nome, email FROM public.funcionario WHERE funcionario_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Funcionario
 {
 FuncionarioId = reader.GetInt32(0),
 Nome = reader.IsDBNull(1) ? null : reader.GetString(1),
 Email = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.funcionario WHERE funcionario_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}