using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;

namespace PIM_FINAL.Controllers
{
 public class StatusChamadoController : Controller
 {
 private readonly string _connectionString;

 public StatusChamadoController(IConfiguration config)
 {
 _connectionString = config["SUPABASE_DB_CONNECTION"] ?? System.Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty;
 }

 public IActionResult Index()
 {
 var list = new List<StatusChamado>();
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT status_id, nome_status, descricao FROM public.status_chamado ORDER BY status_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new StatusChamado
 {
 StatusId = reader.GetInt32(0),
 NomeStatus = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 });
 }
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT status_id, nome_status, descricao FROM public.status_chamado WHERE status_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new StatusChamado
 {
 StatusId = reader.GetInt32(0),
 NomeStatus = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 public IActionResult Create()
 {
 return View(new StatusChamado());
 }

 [HttpPost]
 public IActionResult Create(StatusChamado model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("INSERT INTO public.status_chamado (nome_status, descricao) VALUES (@nome, @descricao)", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomeStatus ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT status_id, nome_status, descricao FROM public.status_chamado WHERE status_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new StatusChamado
 {
 StatusId = reader.GetInt32(0),
 NomeStatus = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(StatusChamado model)
 {
 if (!ModelState.IsValid) return View(model);
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("UPDATE public.status_chamado SET nome_status = @nome, descricao = @descricao WHERE status_id = @id", conn);
 cmd.Parameters.AddWithValue("@nome", (object?)model.NomeStatus ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@descricao", (object?)model.Descricao ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.StatusId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("SELECT status_id, nome_status, descricao FROM public.status_chamado WHERE status_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new StatusChamado
 {
 StatusId = reader.GetInt32(0),
 NomeStatus = reader.IsDBNull(1) ? null : reader.GetString(1),
 Descricao = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = new NpgsqlConnection(_connectionString);
 conn.Open();
 using var cmd = new NpgsqlCommand("DELETE FROM public.status_chamado WHERE status_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}