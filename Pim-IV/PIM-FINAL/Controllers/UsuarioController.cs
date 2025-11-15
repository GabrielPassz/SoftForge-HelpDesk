using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PIM_FINAL.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace PIM_FINAL.Controllers
{
 public class UsuarioController : Controller
 {
 private readonly string _connectionString;
 public UsuarioController(IConfiguration config)
 { _connectionString = config["SUPABASE_DB_CONNECTION"] ?? Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION") ?? string.Empty; }
 private NpgsqlConnection OpenConn(){ var c=new NpgsqlConnection(_connectionString); c.Open(); return c; }

 private void LoadDropdowns(int? selectedDep=null, int? selectedPerf=null)
 {
 using var conn = OpenConn();
 var deps = new List<SelectListItem>();
 using (var cmd = new NpgsqlCommand("SELECT departamento_id, nome_departamento FROM public.departamento ORDER BY nome_departamento", conn))
 using (var r = cmd.ExecuteReader())
 { while (r.Read()){ var id=r.GetInt32(0); var nome = r.IsDBNull(1)?("Departamento #"+id): r.GetString(1); deps.Add(new SelectListItem(nome,id.ToString())); } }
 ViewBag.Departamentos = deps; ViewBag.SelectedDepartamento = selectedDep?.ToString();
 var perfis = new List<SelectListItem>();
 using (var cmd = new NpgsqlCommand("SELECT perfil_id, nome_perfil FROM public.perfil_usuario ORDER BY perfil_id", conn))
 using (var r = cmd.ExecuteReader())
 { while (r.Read()){ var id=r.GetInt32(0); var nome = r.IsDBNull(1)?("Perfil #"+id): r.GetString(1); perfis.Add(new SelectListItem(nome,id.ToString())); } }
 ViewBag.Perfis = perfis; ViewBag.SelectedPerfil = selectedPerf?.ToString();
 }

 private void LoadNameMaps()
 {
 using var conn = OpenConn();
 var mapDep = new Dictionary<int,string>();
 using (var cmd = new NpgsqlCommand("SELECT departamento_id, nome_departamento FROM public.departamento", conn))
 using (var r = cmd.ExecuteReader()){ while(r.Read()) mapDep[r.GetInt32(0)] = r.IsDBNull(1)?("Departamento #"+r.GetInt32(0)): r.GetString(1); }
 var mapPerf = new Dictionary<int,string>();
 using (var cmd = new NpgsqlCommand("SELECT perfil_id, nome_perfil FROM public.perfil_usuario", conn))
 using (var r = cmd.ExecuteReader()){ while(r.Read()) mapPerf[r.GetInt32(0)] = r.IsDBNull(1)?("Perfil #"+r.GetInt32(0)): r.GetString(1); }
 ViewBag.MapDepartamentos = mapDep; ViewBag.MapPerfis = mapPerf;
 }

 public IActionResult Index()
 {
 var list = new List<Usuario>();
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand("SELECT usuario_id, nome_completo, email, data_cadastro, ultimo_login, departamento_id, perfil_id FROM public.usuario ORDER BY usuario_id", conn);
 using var reader = cmd.ExecuteReader();
 while (reader.Read())
 {
 list.Add(new Usuario
 {
 UsuarioId = reader.GetInt32(0),
 NomeCompleto = reader.IsDBNull(1) ? null : reader.GetString(1),
 Email = reader.IsDBNull(2) ? null : reader.GetString(2),
 DataCadastro = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
 UltimoLogin = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 DepartamentoId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
 PerfilId = reader.IsDBNull(6) ? null : reader.GetInt32(6)
 });
 }
 LoadNameMaps();
 return View(list);
 }

 public IActionResult Details(int id)
 {
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand("SELECT usuario_id, nome_completo, email, data_cadastro, ultimo_login, departamento_id, perfil_id FROM public.usuario WHERE usuario_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Usuario
 {
 UsuarioId = reader.GetInt32(0),
 NomeCompleto = reader.IsDBNull(1) ? null : reader.GetString(1),
 Email = reader.IsDBNull(2) ? null : reader.GetString(2),
 DataCadastro = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
 UltimoLogin = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 DepartamentoId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
 PerfilId = reader.IsDBNull(6) ? null : reader.GetInt32(6)
 };
 LoadNameMaps();
 return View(model);
 }

 public IActionResult Create()
 {
 LoadDropdowns();
 return View(new Usuario{ DataCadastro = DateTime.UtcNow });
 }

 [HttpPost]
 public IActionResult Create(Usuario model)
 {
 if (!ModelState.IsValid){ LoadDropdowns(model.DepartamentoId, model.PerfilId); return View(model); }
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand(@"INSERT INTO public.usuario (nome_completo, email, data_cadastro, ultimo_login, departamento_id, perfil_id)
 VALUES (@nome_completo, @email, @data_cadastro, @ultimo_login, @departamento_id, @perfil_id)", conn);
 cmd.Parameters.AddWithValue("@nome_completo", (object?)model.NomeCompleto ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@email", (object?)model.Email ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_cadastro", (object?)model.DataCadastro ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@ultimo_login", (object?)model.UltimoLogin ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@departamento_id", (object?)model.DepartamentoId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@perfil_id", (object?)model.PerfilId ?? System.DBNull.Value);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Edit(int id)
 {
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand("SELECT usuario_id, nome_completo, email, data_cadastro, ultimo_login, departamento_id, perfil_id FROM public.usuario WHERE usuario_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Usuario
 {
 UsuarioId = reader.GetInt32(0),
 NomeCompleto = reader.IsDBNull(1) ? null : reader.GetString(1),
 Email = reader.IsDBNull(2) ? null : reader.GetString(2),
 DataCadastro = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
 UltimoLogin = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
 DepartamentoId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
 PerfilId = reader.IsDBNull(6) ? null : reader.GetInt32(6)
 };
 LoadDropdowns(model.DepartamentoId, model.PerfilId);
 return View(model);
 }

 [HttpPost]
 public IActionResult Edit(Usuario model)
 {
 if (!ModelState.IsValid){ LoadDropdowns(model.DepartamentoId, model.PerfilId); return View(model); }
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand(@"UPDATE public.usuario SET nome_completo = @nome_completo, email = @email, data_cadastro = @data_cadastro, ultimo_login = @ultimo_login, departamento_id = @departamento_id, perfil_id = @perfil_id WHERE usuario_id = @id", conn);
 cmd.Parameters.AddWithValue("@nome_completo", (object?)model.NomeCompleto ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@email", (object?)model.Email ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@data_cadastro", (object?)model.DataCadastro ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@ultimo_login", (object?)model.UltimoLogin ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@departamento_id", (object?)model.DepartamentoId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@perfil_id", (object?)model.PerfilId ?? System.DBNull.Value);
 cmd.Parameters.AddWithValue("@id", model.UsuarioId);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }

 public IActionResult Delete(int id)
 {
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand("SELECT usuario_id, nome_completo, email FROM public.usuario WHERE usuario_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 using var reader = cmd.ExecuteReader();
 if (!reader.Read()) return NotFound();
 var model = new Usuario
 {
 UsuarioId = reader.GetInt32(0),
 NomeCompleto = reader.IsDBNull(1) ? null : reader.GetString(1),
 Email = reader.IsDBNull(2) ? null : reader.GetString(2)
 };
 return View(model);
 }

 [HttpPost, ActionName("Delete")]
 public IActionResult DeleteConfirmed(int id)
 {
 using var conn = OpenConn();
 using var cmd = new NpgsqlCommand("DELETE FROM public.usuario WHERE usuario_id = @id", conn);
 cmd.Parameters.AddWithValue("@id", id);
 cmd.ExecuteNonQuery();
 return RedirectToAction(nameof(Index));
 }
 }
}