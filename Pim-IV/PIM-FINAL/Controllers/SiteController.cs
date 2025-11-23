using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using PIM_FINAL.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using PIM_FINAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text;
using PIM_FINAL.Services; // AI service
using System.Globalization;
using Npgsql; // catch provider-specific exceptions

namespace PIM_FINAL.Controllers
{
 public class SiteController : Controller
 {
 private readonly IWebHostEnvironment _env;
 private readonly PIMContext _db;
 private readonly IConfiguration _config;
 private readonly IAiService _ai; // injected AI
 private const long MaxUploadBytes =5 *1024 *1024; //5MB
 private static readonly string[] AllowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".txt", ".log", ".doc", ".docx" };

 public SiteController(IWebHostEnvironment env, PIMContext db, IConfiguration config, IAiService ai)
 {
 _env = env;
 _db = db;
 _config = config;
 _ai = ai;

 // ensure uploads folder exists
 var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
 if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
 }

 // Allow URLs like /Site or /Site/{viewName} or /Site/{viewName}/{id}
 [HttpGet]
 [AllowAnonymous]
 [Route("Site")]
 [Route("Site/{viewName}")]
 [Route("Site/{viewName}/{id}")]
 public IActionResult Page(string viewName = "InicialPainel", string id = null)
 {
 // sanitize viewName to avoid path traversal
 viewName = Path.GetFileName(viewName ?? string.Empty);
 if (string.IsNullOrEmpty(viewName))
 viewName = "InicialPainel";

 // pages that can be accessed without authentication
 var anonymousPages = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
 {
 "Login",
 "Cadastro",
 "RegistroCadastro",
 "RecuperarSenha",
 "ValidarCodigoRedefinir"
 };

 // if user is not authenticated and the requested page is NOT in the anonymous list, redirect to login
 var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
 if (!isAuthenticated && !anonymousPages.Contains(viewName))
 {
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 // If requesting the inicial panel, redirect to the action that builds its viewmodel
 if (string.Equals(viewName, "InicialPainel", StringComparison.OrdinalIgnoreCase))
 {
 // If already inside InicialPainel action avoid redirect loop
 var currentAction = (string)RouteData.Values["action"] ?? string.Empty;
 if (!string.Equals(currentAction, "InicialPainel", StringComparison.OrdinalIgnoreCase))
 {
 return RedirectToAction("InicialPainel");
 }
 }

 var viewPath = Path.Combine(_env.ContentRootPath, "Views", "site", viewName + ".cshtml");
 if (!System.IO.File.Exists(viewPath))
 {
 return NotFound();
 }

 // if an id was provided, add it to RouteData so views that read RouteData.Values["id"] work
 if (!string.IsNullOrEmpty(id))
 {
 RouteData.Values["id"] = id;
 }

 // expose id to ViewData/ViewBag to avoid nulls in views
 var idValue = id ?? (RouteData.Values.ContainsKey("id") ? RouteData.Values["id"]?.ToString() : null);
 if (!string.IsNullOrEmpty(idValue))
 {
 ViewData["id"] = idValue;
 ViewBag.Id = idValue;
 }

 // Special handling: strongly typed view requiring model
 if (string.Equals(viewName, "EditarUsuario", StringComparison.OrdinalIgnoreCase))
 {
 // Only admin (PerfilId1) allowed via this shortcut
 var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
 if (roleClaim != "1") return Forbid();
 var currentIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(currentIdStr, out var currentId)) return RedirectToAction("Page", new { viewName = "Login" });
 var userEntity = _db.Usuarios.Include(u=>u.Departamento).Include(u=>u.PerfilUsuario).FirstOrDefault(u=>u.UsuarioId==currentId);
 if (userEntity == null) return NotFound();
 var perfis = _db.PerfisUsuario.OrderBy(p=>p.NomePerfil).ToList();
 var deps = _db.Departamentos.OrderBy(d=>d.NomeDepartamento).ToList();
 var vm = new EditUsuarioViewModel
 {
 UsuarioId = userEntity.UsuarioId,
 NomeCompleto = userEntity.NomeCompleto,
 Email = userEntity.Email,
 PerfilId = userEntity.PerfilId,
 DepartamentoId = userEntity.DepartamentoId,
 Perfis = perfis.Select(p=> new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(p.NomePerfil, p.PerfilId.ToString())).ToList(),
 Departamentos = deps.Select(d=> new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(d.NomeDepartamento, d.DepartamentoId.ToString())).ToList()
 };
 return View("~/Views/site/EditarUsuario.cshtml", vm);
 }

 return View($"~/Views/site/{viewName}.cshtml");
 }

 [HttpPost]
 [AllowAnonymous]
 [Route("Site/Login")]
 public async Task<IActionResult> Login(string usuario, string senha, string returnUrl = null)
 {
 try
 {
 if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
 {
 TempData["LoginError"] = "Informe usuário e senha.";
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == usuario || u.NomeCompleto == usuario);
 if (user == null)
 {
 TempData["LoginError"] = "Usuário ou senha inválidos.";
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 var hasher = new PasswordHasher<Usuario>();
 bool valid = false;

 //1) Tenta verificar como hash do ASP.NET Identity (pode lançar FormatException se não for hash válido)
 try
 {
 if (!string.IsNullOrEmpty(user.Senha))
 {
 var verify = hasher.VerifyHashedPassword(user, user.Senha, senha);
 if (verify == PasswordVerificationResult.Success || verify == PasswordVerificationResult.SuccessRehashNeeded)
 {
 valid = true;
 if (verify == PasswordVerificationResult.SuccessRehashNeeded)
 {
 user.Senha = hasher.HashPassword(user, senha);
 await _db.SaveChangesAsync();
 }
 }
 }
 }
 catch (FormatException)
 {
 // Vai tratar nos fallbacks abaixo
 }

 //2) Fallback: senha armazenada em texto puro
 if (!valid && !string.IsNullOrEmpty(user.Senha) && user.Senha == senha)
 {
 user.Senha = hasher.HashPassword(user, senha);
 await _db.SaveChangesAsync();
 valid = true;
 }

 //3) Fallback: senha armazenada como Base64 de texto
 if (!valid && !string.IsNullOrEmpty(user.Senha))
 {
 try
 {
 var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(user.Senha));
 if (decoded == senha)
 {
 user.Senha = hasher.HashPassword(user, senha);
 await _db.SaveChangesAsync();
 valid = true;
 }
 }
 catch { /* ignore */ }
 }

 if (!valid)
 {
 TempData["LoginError"] = "Usuário ou senha inválidos.";
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 // Bloqueia usuários pendentes (sem perfil definido)
 if (!user.PerfilId.HasValue)
 {
 TempData["LoginError"] = "Sua conta ainda aguarda aprovação do administrador.";
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 // Claims e login
 var claims = new List<Claim>
 {
 new Claim(ClaimTypes.NameIdentifier, user.UsuarioId.ToString()),
 new Claim(ClaimTypes.Name, user.NomeCompleto ?? string.Empty),
 new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
 new Claim(ClaimTypes.Role, user.PerfilId.Value.ToString())
 };

 var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
 var principal = new ClaimsPrincipal(claimsIdentity);
 await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

 user.UltimoLogin = DateTime.UtcNow;
 await _db.SaveChangesAsync();
 // Garantir flush para outros contextos antes do redirect
 await _db.Entry(user).ReloadAsync();

 // redirect to returnUrl if local
 if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
 {
 return Redirect(returnUrl);
 }

 return RedirectToAction("Page", new { viewName = "InicialPainel" });
 }
 catch (TimeoutException)
 {
 TempData["LoginError"] = "Banco de dados indisponível (timeout). Tente novamente em instantes.";
 return RedirectToAction("Page", new { viewName = "Login" });
 }
 catch (NpgsqlException)
 {
 TempData["LoginError"] = "Falha ao conectar ao banco de dados. Verifique a conexão e tente novamente.";
 return RedirectToAction("Page", new { viewName = "Login" });
 }
 catch (InvalidOperationException)
 {
 TempData["LoginError"] = "Serviço temporariamente indisponível. Tente novamente.";
 return RedirectToAction("Page", new { viewName = "Login" });
 }
 }

 [HttpPost]
 [Route("Site/Logout")]
 public async Task<IActionResult> Logout()
 {
 await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 // Registration: create pending user (requires admin approval)
 [HttpPost]
 [AllowAnonymous]
 [Route("Site/RegistroCadastro")]
 public async Task<IActionResult> RegistroCadastro(string nome, string email, string senha)
 {
 if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
 {
 TempData["ErrorMessage"] = "Preencha todos os campos.";
 return RedirectToAction("Page", new { viewName = "Cadastro" });
 }

 var existing = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
 if (existing != null)
 {
 TempData["ErrorMessage"] = "E-mail já cadastrado.";
 return RedirectToAction("Page", new { viewName = "Cadastro" });
 }

 var user = new Usuario
 {
 NomeCompleto = nome,
 Email = email,
 DataCadastro = DateTime.UtcNow,
 PerfilId = null // pendente até aprovação
 };

 var hasher = new PasswordHasher<Usuario>();
 user.Senha = hasher.HashPassword(user, senha);

 _db.Usuarios.Add(user);
 await _db.SaveChangesAsync();

 TempData["SuccessMessage"] = "Solicitação enviada. Aguarde aprovação do administrador.";
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 // Abrir Chamado - GET
 [HttpGet]
 [Authorize]
 [Route("Site/AbrirChamado")]
 public async Task<IActionResult> AbrirChamado()
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });

 var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == userId);
 var model = new Models.AbrirChamado
 {
 SolicitanteNome = user?.NomeCompleto ?? string.Empty,
 SolicitanteDepartamento = user?.Departamento?.NomeDepartamento ?? string.Empty
 };

 var categorias = await _db.Categorias.OrderBy(c => c.NomeCategoria).ToListAsync();
 model.CategoriasSelect = categorias.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(c.NomeCategoria, c.CategoriaId.ToString())).ToList();
 var prioridades = await _db.Prioridades.OrderBy(p => p.NomePrioridade).ToListAsync();
 model.PrioridadesSelect = prioridades.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(p.NomePrioridade, p.PrioridadeId.ToString())).ToList();

 return View("~/Views/site/AbrirChamado.cshtml", model);
 }

 // Abrir Chamado - POST (com IA)
 [HttpPost]
 [Authorize]
 [Route("Site/AbrirChamado")]
 public async Task<IActionResult> AbrirChamado([FromForm] Models.Chamado chamado, IFormFile? AnexoFile)
 {
 chamado.DataAbertura = DateTime.UtcNow;
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });
 chamado.UsuarioSolicitanteId = userId;
 chamado.Protocolo = $"CHAM-{DateTime.UtcNow:yyyy}-{new Random().Next(100,999)}";

 // Flags de escolha automática (valor0 vindo do select)
 bool categoriaAuto = chamado.CategoriaId ==0;
 bool prioridadeAuto = chamado.PrioridadeId ==0;
 if (categoriaAuto) chamado.CategoriaId = null; // evitar persistir0
 if (prioridadeAuto) chamado.PrioridadeId = null;

 // IA: somente classificar se pelo menos um campo estiver em modo automático
 if (categoriaAuto || prioridadeAuto)
 {
 var categoriasNomes = await _db.Categorias.OrderBy(c => c.NomeCategoria)
 .Select(c => new { c.CategoriaId, c.NomeCategoria })
 .ToListAsync();
 var prioridadesNomes = await _db.Prioridades.OrderBy(p => p.NomePrioridade)
 .Select(p => new { p.PrioridadeId, p.NomePrioridade })
 .ToListAsync();
 try
 {
 var classification = await _ai.ClassifyAsync(chamado.Titulo ?? string.Empty, chamado.Descricao ?? string.Empty,
 categoriasNomes.Select(c => c.NomeCategoria), prioridadesNomes.Select(p => p.NomePrioridade));
 var cat = classification.Categoria; var pri = classification.Prioridade; var just = classification.Justificativa;
 var mapCat = categoriasNomes.ToDictionary(c => NormalizeKey(c.NomeCategoria), c => c.CategoriaId);
 var mapPri = prioridadesNomes.ToDictionary(p => NormalizeKey(p.NomePrioridade), p => p.PrioridadeId);
 if (categoriaAuto && !string.IsNullOrWhiteSpace(cat))
 {
 var key = NormalizeKey(cat);
 if (!mapCat.TryGetValue(key, out var cid))
 {
 var hit = mapCat.Keys.FirstOrDefault(k => k.Contains(key) || key.Contains(k));
 if (hit != null) cid = mapCat[hit];
 }
 if (cid !=0) chamado.CategoriaId = cid;
 }
 if (prioridadeAuto && !string.IsNullOrWhiteSpace(pri))
 {
 var key = NormalizeKey(pri);
 // sinônimos em inglês -> pt-br
 if (!mapPri.ContainsKey(key))
 {
 var synonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
 {
 ["critical"] = "critica",
 ["urgent"] = "critica",
 ["high"] = "alta",
 ["medium"] = "media",
 ["normal"] = "media",
 ["low"] = "baixa",
 ["lowest"] = "baixa",
 ["highest"] = "critica"
 };
 if (synonyms.TryGetValue(key, out var mappedKey)) key = mappedKey;
 }
 if (!mapPri.TryGetValue(key, out var pid))
 {
 var hit = mapPri.Keys.FirstOrDefault(k => k.Contains(key) || key.Contains(k));
 if (hit != null) pid = mapPri[hit];
 }
 if (pid !=0) chamado.PrioridadeId = pid;
 }
 if (!string.IsNullOrWhiteSpace(just) && (categoriaAuto || prioridadeAuto))
 {
 // será salvo após salvar chamado
 HttpContext.Items["IA_CLASS_JUST"] = just;
 HttpContext.Items["IA_CLASS_CAT"] = cat;
 }
 }
 catch { }
 }

 _db.Chamados.Add(chamado);
 await _db.SaveChangesAsync();

 // Registrar análise IA se houver justificativa capturada
 if (HttpContext.Items.ContainsKey("IA_CLASS_JUST"))
 {
 try
 {
 var analise = new IaAnalise
 {
 ChamadoId = chamado.ChamadoId,
 CategoriaPrevista = HttpContext.Items["IA_CLASS_CAT"] as string,
 Comentario = HttpContext.Items["IA_CLASS_JUST"] as string,
 DataAnalise = DateTime.UtcNow
 };
 _db.Add(analise);
 await _db.SaveChangesAsync();
 }
 catch { }
 }

 // IA resposta inicial (sempre pode ocorrer)
 try
 {
 var reply = await _ai.SuggestInitialReplyAsync(chamado.Titulo ?? string.Empty, chamado.Descricao ?? string.Empty);
 if (!string.IsNullOrWhiteSpace(reply))
 {
 var comunicacao = new Comunicacao
 {
 ChamadoId = chamado.ChamadoId,
 UsuarioId = userId,
 Mensagem = "[IA] " + reply,
 DataEnvio = DateTime.UtcNow
 };
 _db.Comunicacoes.Add(comunicacao);
 await _db.SaveChangesAsync();
 }
 }
 catch { }

 // IA: RAG recomendação de artigos e chamados similares
 try
 {
 var keywords = await _ai.ExtractKeywordsAsync($"{chamado.Titulo} {chamado.Descricao}");
 if (keywords.Length >0)
 {
 var kws = keywords.Select(k => k.ToLower()).ToList();
 var artigos = await _db.BasesConhecimento
 .Where(a => a.Aprovado == true && (
 kws.Any(k => (a.Titulo ?? "").ToLower().Contains(k)) ||
 kws.Any(k => (a.Descricao ?? "").ToLower().Contains(k)) ||
 kws.Any(k => (a.Solucao ?? "").ToLower().Contains(k))
 ))
 .OrderByDescending(a => a.BaseId)
 .Take(3)
 .ToListAsync();
 if (artigos.Any())
 {
 var lines = artigos.Select(a => $"- {a.Titulo} (/BaseConhecimento/Details/{a.BaseId})");
 var msg = "[IA] Artigos relacionados:\n" + string.Join("\n", lines);
 _db.Comunicacoes.Add(new Comunicacao { ChamadoId = chamado.ChamadoId, UsuarioId = userId, Mensagem = msg, DataEnvio = DateTime.UtcNow });
 await _db.SaveChangesAsync();
 }
 var similares = await _db.Chamados
 .OrderByDescending(c => c.ChamadoId)
 .Take(100)
 .Where(c => c.ChamadoId != chamado.ChamadoId && (
 kws.Any(k => (c.Titulo ?? "").ToLower().Contains(k)) ||
 kws.Any(k => (c.Descricao ?? "").ToLower().Contains(k))
 ))
 .Take(3)
 .ToListAsync();
 if (similares.Any())
 {
 var lines = similares.Select(c => $"- {(c.Protocolo ?? "#")} {c.Titulo}");
 var msg = "[IA] Chamados possivelmente relacionados:\n" + string.Join("\n", lines);
 _db.Comunicacoes.Add(new Comunicacao { ChamadoId = chamado.ChamadoId, UsuarioId = userId, Mensagem = msg, DataEnvio = DateTime.UtcNow });
 await _db.SaveChangesAsync();
 }
 }
 }
 catch { }

 TempData["SuccessMessage"] = $"Chamado criado: {chamado.Protocolo}";

 if (AnexoFile != null && AnexoFile.Length >0)
 {
 try
 {
 var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
 if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
 var uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(AnexoFile.FileName)}";
 var filePath = Path.Combine(uploadsDir, uniqueFileName);
 using (var stream = System.IO.File.Create(filePath))
 await AnexoFile.CopyToAsync(stream);
 var anexo = new Anexo
 {
 ChamadoId = chamado.ChamadoId,
 NomeArquivo = AnexoFile.FileName,
 TipoArquivo = AnexoFile.ContentType,
 CaminhoArquivo = "/uploads/" + uniqueFileName,
 DataUpload = DateTime.UtcNow,
 UsuarioId = userId
 };
 _db.Anexos.Add(anexo);
 await _db.SaveChangesAsync();
 }
 catch { }
 }

 try { await SendNotificationEmailAsync((await _db.Usuarios.FindAsync(chamado.UsuarioSolicitanteId))?.Email, "Seu chamado foi criado", $"Seu chamado {chamado.Protocolo} foi criado."); } catch { }
 return RedirectToAction("MeusChamados");
 }

 // MeusChamados - list authenticated user's chamados
 [HttpGet]
 [Authorize]
 [Route("Site/MeusChamados")]
 public async Task<IActionResult> MeusChamados(string? status = null, string? prioridade = null, string? busca = null)
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId))
 {
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 var query = _db.Chamados
 .Include(c => c.Prioridade)
 .Include(c => c.StatusChamado)
 .Include(c => c.Categoria)
 .Where(c => c.UsuarioSolicitanteId == userId);

 if (!string.IsNullOrWhiteSpace(status))
 {
 switch (status.ToLowerInvariant())
 {
 case "aberto":
 query = query.Where(c => c.StatusChamado == null || (c.StatusChamado.NomeStatus != null && EF.Functions.ILike(c.StatusChamado.NomeStatus, "%abert%")));
 break;
 case "em_atendimento":
 query = query.Where(c => c.StatusChamado != null && c.StatusChamado.NomeStatus != null && EF.Functions.ILike(c.StatusChamado.NomeStatus, "%atend%"));
 break;
 case "aguardando_validacao":
 query = query.Where(c => c.StatusChamado != null && c.StatusChamado.NomeStatus != null && (EF.Functions.ILike(c.StatusChamado.NomeStatus, "%valid%") || EF.Functions.ILike(c.StatusChamado.NomeStatus, "%valida%")));
 break;
 case "fechado":
 query = query.Where(c => c.DataFechamento != null);
 break;
 }
 }

 if (!string.IsNullOrWhiteSpace(prioridade))
 {
 // Tornar filtro de prioridade robusto a acentos mapeando para o ID correspondente
 var prios = await _db.Prioridades.AsNoTracking().ToListAsync();
 var targetKey = NormalizeKey(prioridade);
 var match = prios.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.NomePrioridade) && NormalizeKey(p.NomePrioridade).Contains(targetKey));
 if (match != null)
 {
 query = query.Where(c => c.PrioridadeId == match.PrioridadeId);
 }
 else
 {
 // fallback: tentativa ampla por nome
 query = query.Where(c => c.Prioridade != null && c.Prioridade.NomePrioridade != null && EF.Functions.ILike(c.Prioridade.NomePrioridade, $"%{prioridade}%"));
 }
 }

 if (!string.IsNullOrWhiteSpace(busca))
 {
 var term = busca.Trim();
 query = query.Where(c =>
 (c.Protocolo != null && EF.Functions.ILike(c.Protocolo, $"%{term}%")) ||
 (c.Titulo != null && EF.Functions.ILike(c.Titulo, $"%{term}%")) ||
 (c.Descricao != null && EF.Functions.ILike(c.Descricao, $"%{term}%"))
 );
 }

 var chamados = await query.OrderByDescending(c => c.DataAbertura).ToListAsync();
 ViewBag.StatusFilter = status;
 ViewBag.PrioridadeFilter = prioridade;
 ViewBag.BuscaFilter = busca;
 return View("~/Views/site/MeusChamados.cshtml", chamados);
 }

 // Detalhes do chamado do usuário
 [HttpGet]
 [Authorize]
 [Route("Site/DetalhesMeuChamado/{id}")]
 public async Task<IActionResult> DetalhesMeuChamado(int id)
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId))
 {
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 var chamado = await _db.Chamados
 .Include(c => c.Prioridade)
 .Include(c => c.StatusChamado)
 .Include(c => c.Categoria)
 .Include(c => c.Anexos)
 .Include(c => c.Comunicacoes).ThenInclude(cm => cm.Usuario)
 .Include(c => c.Atendimentos).ThenInclude(a => a.UsuarioTecnico) // incluir histórico
 .FirstOrDefaultAsync(c => c.ChamadoId == id);

 if (chamado == null) return NotFound();
 if (chamado.UsuarioSolicitanteId != userId) return Forbid();

 return View("~/Views/site/DetalhesMeuChamado.cshtml", chamado);
 }

 // Technician/manager/admin view of a chamado
 [HttpGet]
 [Authorize]
 [Route("Site/DetalhesChamado/{id}")]
 public async Task<IActionResult> DetalhesChamado(int id)
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId))
 {
 return RedirectToAction("Page", new { viewName = "Login" });
 }

 var chamado = await _db.Chamados
 .Include(c => c.Prioridade)
 .Include(c => c.StatusChamado)
 .Include(c => c.Categoria)
 .Include(c => c.Anexos)
 .Include(c => c.Comunicacoes).ThenInclude(cm => cm.Usuario)
 .Include(c => c.Atendimentos).ThenInclude(a => a.UsuarioTecnico)
 .Include(c => c.UsuarioSolicitante)
 .Include(c => c.TecnicoResponsavel)
 .FirstOrDefaultAsync(c => c.ChamadoId == id);

 if (chamado == null) return NotFound();

 // permit: assigned technician, manager/admin (roles1,2), any technician (3) or the owner
 var role = User.FindFirst(ClaimTypes.Role)?.Value;
 bool isTecnico = role == "3" || role == "2" || role == "1";
 if (chamado.TecnicoResponsavelId != userId && !isTecnico && chamado.UsuarioSolicitanteId != userId)
 {
 return Forbid();
 }

 return View("~/Views/site/DetalhesChamado.cshtml", chamado);
 }

 // Escalonamento: manual form POST from Escalonamento.cshtml
 [HttpPost]
 [Authorize]
 [Route("Site/Escalonamento")]
 public async Task<IActionResult> Escalonamento(string protocolo, string motivo, string novo_tecnico, string nova_prioridade)
 {
 if (string.IsNullOrWhiteSpace(protocolo))
 {
 TempData["ErrorMessage"] = "Protocolo é obrigatório.";
 return RedirectToAction("Page", new { viewName = "Escalonamento" });
 }

 // garantir que somente técnico/gestor/admin execute
 var role = User.FindFirst(ClaimTypes.Role)?.Value;
 bool isTecnico = role == "3" || role == "2" || role == "1";
 if (!isTecnico) return Forbid();

 var proto = protocolo.Trim();
 var proto2 = proto.TrimStart('#');
 var chamado = await _db.Chamados.FirstOrDefaultAsync(c => c.Protocolo != null && (
 EF.Functions.ILike(c.Protocolo, proto) ||
 EF.Functions.ILike(c.Protocolo, proto2) ||
 EF.Functions.ILike(c.Protocolo, $"%{proto2}%")
));
 if (chamado == null)
 {
 TempData["ErrorMessage"] = "Chamado não encontrado.";
 return RedirectToAction("Page", new { viewName = "Escalonamento" });
 }

 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });

 // resolve technician by id or name
 if (!string.IsNullOrWhiteSpace(novo_tecnico))
 {
 if (int.TryParse(novo_tecnico, out var parsed)) chamado.TecnicoResponsavelId = parsed;
 else
 {
 var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.NomeCompleto != null && EF.Functions.ILike(x.NomeCompleto, $"%{novo_tecnico}%"));
 if (u != null) chamado.TecnicoResponsavelId = u.UsuarioId;
 }
 }

 if (!string.IsNullOrWhiteSpace(nova_prioridade))
 {
 // normaliza acentos para casar valores como "critica" com "Crítica"
 var prios = await _db.Prioridades.ToListAsync();
 var targetKey = NormalizeKey(nova_prioridade);
 var p = prios.FirstOrDefault(x => x.NomePrioridade != null && NormalizeKey(x.NomePrioridade).Contains(targetKey));
 if (p != null) chamado.PrioridadeId = p.PrioridadeId;
 }

 var comunicacao = new Comunicacao
 {
 ChamadoId = chamado.ChamadoId,
 UsuarioId = userId,
 Mensagem = $"Escalonamento manual: {motivo}",
 DataEnvio = DateTime.UtcNow
 };
 _db.Comunicacoes.Add(comunicacao);
 await _db.SaveChangesAsync();

 TempData["SuccessMessage"] = "Escalonamento realizado.";
 return RedirectToAction("DetalhesEscalonamento", new { id = chamado.ChamadoId });
 }

 // Escalonar chamado (route used by form '/escalonar-chamado')
 [HttpPost]
 [Authorize]
 [Route("Site/EscalonarChamado")] // garantir compatibilidade com asp-controller="Site" asp-action="EscalonarChamado"
 public async Task<IActionResult> EscalonarChamado(int chamado_id, string novo_tecnico, string motivo, string nova_prioridade)
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });
 var chamado = await _db.Chamados.FirstOrDefaultAsync(c => c.ChamadoId == chamado_id);
 if (chamado == null) return NotFound();

 // resolve novo_tecnico either as id or name
 int novoTecnicoId =0;
 if (int.TryParse(novo_tecnico, out var parsed)) novoTecnicoId = parsed;
 else if (!string.IsNullOrWhiteSpace(novo_tecnico))
 {
 var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.NomeCompleto != null && x.NomeCompleto.ToLower().Contains(novo_tecnico.ToLower()));
 if (u != null) novoTecnicoId = u.UsuarioId;
 }
 if (novoTecnicoId >0) chamado.TecnicoResponsavelId = novoTecnicoId;

 if (!string.IsNullOrWhiteSpace(nova_prioridade))
 {
 // matching resiliente a acentos
 var prios = await _db.Prioridades.ToListAsync();
 var targetKey = NormalizeKey(nova_prioridade);
 var p = prios.FirstOrDefault(x => x.NomePrioridade != null && NormalizeKey(x.NomePrioridade).Contains(targetKey));
 if (p != null) chamado.PrioridadeId = p.PrioridadeId;
 }

 // add history communication
 var comunicacao = new Comunicacao
 {
 ChamadoId = chamado.ChamadoId,
 UsuarioId = userId,
 Mensagem = $"Escalonado: {motivo}",
 DataEnvio = DateTime.UtcNow
 };
 _db.Comunicacoes.Add(comunicacao);
 await _db.SaveChangesAsync();

 TempData["SuccessMessage"] = "Chamado escalonado.";
 try { await SendNotificationEmailAsync((await _db.Usuarios.FindAsync(chamado.UsuarioSolicitanteId))?.Email, $"Chamado {chamado.Protocolo} escalonado", motivo); } catch { }

 return RedirectToAction("DetalhesEscalonamento", new { id = chamado.ChamadoId });
 }

 // Atendimento: list for technician (or manager/admin see more)
 [HttpGet]
 [Authorize]
 [Route("Site/Atendimento")]
 [Route("Atendimento")]
 public async Task<IActionResult> Atendimento()
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });
 var role = User.FindFirst(ClaimTypes.Role)?.Value;
 bool isTecnico = role == "3" || role == "2" || role == "1"; // tecnico, gestor, admin

 if (!isTecnico) return Forbid();

 var chamados = await _db.Chamados
 .Include(c => c.Prioridade)
 .Include(c => c.StatusChamado)
 .Include(c => c.Categoria)
 .Include(c => c.UsuarioSolicitante).ThenInclude(u => u.Departamento)
 .Where(c => (c.TecnicoResponsavelId == userId || c.TecnicoResponsavelId == null)
 && c.UsuarioSolicitanteId != userId)
 .OrderByDescending(c => c.DataAbertura)
 .ToListAsync();

 return View("~/Views/site/Atendimento.cshtml", chamados);
 }

 // Start atendimento: assign tecnico and create Atendimento record
 [HttpPost]
 [Authorize]
 [Route("Site/IniciarAtendimento")]
 [Route("iniciar-atendimento")]
 public async Task<IActionResult> IniciarAtendimento(int chamado_id)
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });
 var chamado = await _db.Chamados.FirstOrDefaultAsync(c => c.ChamadoId == chamado_id);
 if (chamado == null) return NotFound();

 chamado.TecnicoResponsavelId = userId;
 // try to set status to "Em Atendimento" if exists
 var status = await _db.StatusChamados.FirstOrDefaultAsync(s => EF.Functions.ILike(s.NomeStatus, "%atendimento%"));
 if (status != null) chamado.StatusId = status.StatusId;

 var atendimento = new Atendimento
 {
 ChamadoId = chamado.ChamadoId,
 UsuarioTecnicoId = userId,
 DataAtendimento = DateTime.UtcNow,
 AcaoRealizada = "Iniciou atendimento",
 TempoGasto =0
 };

 _db.Atendimentos.Add(atendimento);
 await _db.SaveChangesAsync();

 return RedirectToAction("Atendimento");
 }

 // Registrar ação (form action='/registrar-acao')
 [HttpPost]
 [Authorize]
 [Route("registrar-acao")]
 [Route("Site/RegistrarAcao")]
 public async Task<IActionResult> RegistrarAcao(int chamado_id, string acao_realizada, int tempo_gasto, IFormFile? anexo_evidencia)
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });
 var chamado = await _db.Chamados.FirstOrDefaultAsync(c => c.ChamadoId == chamado_id);
 if (chamado == null) return NotFound();
 // Only assigned technician or manager/admin can register (antes de atribuição automática)
 var role = User.FindFirst(ClaimTypes.Role)?.Value;

 // atribuição automática do técnico ao primeiro registro de ação
 bool iniciouAtendimentoAgora = false;
 if (chamado.TecnicoResponsavelId == null)
 {
 chamado.TecnicoResponsavelId = userId; // associa o primeiro técnico que registra ação
 // ajusta status para "Em Atendimento" se existir
 var statusAtendimento = await _db.StatusChamados.FirstOrDefaultAsync(s => EF.Functions.ILike(s.NomeStatus, "%atendimento%"));
 if (statusAtendimento != null) chamado.StatusId = statusAtendimento.StatusId;
 iniciouAtendimentoAgora = true;
 }

 // após possível atribuição, valida permissão: se ainda não é responsável e não é gestor/admin -> bloqueia
 if (chamado.TecnicoResponsavelId != userId && role != "2" && role != "1")
 {
 TempData["ErrorMessage"] = "Você não tem permissão para registrar ação neste chamado.";
 return RedirectToAction("DetalhesChamado", new { id = chamado.ChamadoId });
 }

 var atendimento = new Atendimento
 {
 ChamadoId = chamado.ChamadoId,
 UsuarioTecnicoId = userId,
 DataAtendimento = DateTime.UtcNow,
 AcaoRealizada = acao_realizada,
 TempoGasto = tempo_gasto
 };
 _db.Atendimentos.Add(atendimento);

 // registra comunicação informando início do atendimento, antes da ação, se foi agora
 if (iniciouAtendimentoAgora)
 {
 var tecnico = await _db.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == userId);
 _db.Comunicacoes.Add(new Comunicacao
 {
 ChamadoId = chamado.ChamadoId,
 UsuarioId = userId,
 Mensagem = $"Atendimento iniciado por {tecnico?.NomeCompleto ?? "(técnico)"} em {DateTime.UtcNow.ToLocalTime():g}",
 DataEnvio = DateTime.UtcNow
 });
 }

 await _db.SaveChangesAsync();

 // handle evidence attachment
 if (anexo_evidencia != null && anexo_evidencia.Length >0)
 {
 if (anexo_evidencia.Length > MaxUploadBytes) TempData["ErrorMessage"] = "Arquivo excede o limite de5MB.";
 else
 {
 var ext = Path.GetExtension(anexo_evidencia.FileName).ToLowerInvariant();
 if (!AllowedExtensions.Contains(ext)) TempData["ErrorMessage"] = "Tipo de arquivo não permitido.";
 else
 {
 var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
 var uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(anexo_evidencia.FileName)}";
 var filePath = Path.Combine(uploadsDir, uniqueFileName);
 using (var stream = System.IO.File.Create(filePath)) await anexo_evidencia.CopyToAsync(stream);
 var anexo = new Anexo { ChamadoId = chamado.ChamadoId, NomeArquivo = anexo_evidencia.FileName, TipoArquivo = anexo_evidencia.ContentType, CaminhoArquivo = "/uploads/" + uniqueFileName, DataUpload = DateTime.UtcNow, UsuarioId = userId };
 _db.Anexos.Add(anexo);
 await _db.SaveChangesAsync();
 TempData["SuccessMessage"] = "Ação registrada e anexo salvo.";
 }
 }
 }

 try { await SendNotificationEmailAsync((await _db.Usuarios.FindAsync(chamado.UsuarioSolicitanteId))?.Email, "Atualização no seu chamado", $"Uma nova ação foi registrada no chamado {chamado.Protocolo}."); } catch { }

 // decidir página de retorno: se o usuário é o solicitante, usa DetalhesMeuChamado; caso contrário (técnico/gestor) permanece em DetalhesChamado
 var isOwner = chamado.UsuarioSolicitanteId == userId;
 return isOwner ? RedirectToAction("DetalhesMeuChamado", new { id = chamado.ChamadoId }) : RedirectToAction("DetalhesChamado", new { id = chamado.ChamadoId });
 }

 // Enviar mensagem (comunicacao)
 [HttpPost]
 [Authorize]
 [Route("enviar-mensagem")]
 [Route("Site/EnviarMensagem")]
 public async Task<IActionResult> EnviarMensagem(int chamado_id, string mensagem)
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });
 var chamado = await _db.Chamados.FirstOrDefaultAsync(c => c.ChamadoId == chamado_id);
 if (chamado == null) return NotFound();

 var comunicacao = new Comunicacao
 {
 ChamadoId = chamado.ChamadoId,
 UsuarioId = userId,
 Mensagem = mensagem,
 DataEnvio = DateTime.UtcNow
 };
 _db.Comunicacoes.Add(comunicacao);
 await _db.SaveChangesAsync();

 TempData["SuccessMessage"] = "Mensagem enviada.";
 try { await SendNotificationEmailAsync((await _db.Usuarios.FindAsync(chamado.UsuarioSolicitanteId))?.Email, $"Nova mensagem no chamado {chamado.Protocolo}", mensagem); } catch { }
 var isOwner = chamado.UsuarioSolicitanteId == userId;
 return isOwner ? RedirectToAction("DetalhesMeuChamado", new { id = chamado.ChamadoId }) : RedirectToAction("DetalhesChamado", new { id = chamado.ChamadoId });
 }

 // Resolver chamado
 [HttpPost]
 [Authorize]
 [Route("resolver-chamado")]
 [Route("Site/ResolverChamado")]
 public async Task<IActionResult> ResolverChamado(int chamado_id, string solucao_final)
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });
 var chamado = await _db.Chamados.FirstOrDefaultAsync(c => c.ChamadoId == chamado_id);
 if (chamado == null) return NotFound();
 // only assigned technician or manager/admin
 var role = User.FindFirst(ClaimTypes.Role)?.Value;
 if (chamado.TecnicoResponsavelId != userId && role != "2" && role != "1")
 {
 TempData["ErrorMessage"] = "Você não tem permissão para resolver este chamado.";
 return RedirectToAction("DetalhesChamado", new { id = chamado.ChamadoId });
 }

 chamado.DataFechamento = DateTime.UtcNow;
 // set status to closed if exists
 var status = await _db.StatusChamados.FirstOrDefaultAsync(s => EF.Functions.ILike(s.NomeStatus, "%fechado%"));
 if (status != null) chamado.StatusId = status.StatusId;

 // create atendimento record with final solution
 var atendimento = new Atendimento
 {
 ChamadoId = chamado.ChamadoId,
 UsuarioTecnicoId = userId,
 DataAtendimento = DateTime.UtcNow,
 AcaoRealizada = solucao_final ?? "Resolvido",
 TempoGasto =0
 };
 _db.Atendimentos.Add(atendimento);
 await _db.SaveChangesAsync();

 TempData["SuccessMessage"] = "Chamado marcado como resolvido.";
 try { await SendNotificationEmailAsync((await _db.Usuarios.FindAsync(chamado.UsuarioSolicitanteId))?.Email, $"Chamado {chamado.Protocolo} resolvido", solucao_final); } catch { }
 var isOwner = chamado.UsuarioSolicitanteId == userId;
 return isOwner ? RedirectToAction("DetalhesMeuChamado", new { id = chamado.ChamadoId }) : RedirectToAction("DetalhesChamado", new { id = chamado.ChamadoId });
 }

 // Escalonar chamado (route used by form '/escalonar-chamado')
 [HttpPost]
 [Authorize]
 [Route("devolver-chamado")]
 [Route("Site/DevolverChamado")]
 public async Task<IActionResult> DevolverChamado(int chamado_id, string motivo_devolucao)
 {
 var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Page", new { viewName = "Login" });
 var chamado = await _db.Chamados.FirstOrDefaultAsync(c => c.ChamadoId == chamado_id);
 if (chamado == null) return NotFound();
 // only assigned technician or manager/admin
 var role = User.FindFirst(ClaimTypes.Role)?.Value;
 if (chamado.TecnicoResponsavelId != userId && role != "2" && role != "1")
 {
 TempData["ErrorMessage"] = "Você não tem permissão para devolver este chamado.";
 return RedirectToAction("DetalhesChamado", new { id = chamado.ChamadoId });
 }

 chamado.TecnicoResponsavelId = null;

 var comunicacao = new Comunicacao
 {
 ChamadoId = chamado.ChamadoId,
 UsuarioId = userId,
 Mensagem = $"Devolvido para fila: {motivo_devolucao}",
 DataEnvio = DateTime.UtcNow
 };
 _db.Comunicacoes.Add(comunicacao);
 await _db.SaveChangesAsync();

 TempData["SuccessMessage"] = "Chamado devolvido para fila.";
 try { await SendNotificationEmailAsync((await _db.Usuarios.FindAsync(chamado.UsuarioSolicitanteId))?.Email, $"Chamado {chamado.Protocolo} devolvido", motivo_devolucao); } catch { }

 return RedirectToAction("Atendimento");
 }

 // Aprovar/Reprovar usuário
 [HttpPost]
 [Authorize(Roles = "1")] // only admin
 [Route("Site/AprovarUsuario")]
 [Route("aprovar-usuario")]
 public async Task<IActionResult> AprovarUsuario(int solicitacao_id, string perfil, string departamento)
 {
 // try find user by id or by solicitacao id mapping -- we'll assume solicitacao_id is usuario_id
 var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == solicitacao_id);
 if (user == null) return NotFound();
 // resolve perfil name to id
 var p = await _db.PerfisUsuario.FirstOrDefaultAsync(x => x.NomePerfil != null && EF.Functions.ILike(x.NomePerfil, $"%{perfil}%"));
 if (p != null) user.PerfilId = p.PerfilId;
 // optionally set departamento
 if (!string.IsNullOrWhiteSpace(departamento))
 {
 var d = await _db.Departamentos.FirstOrDefaultAsync(x => x.NomeDepartamento != null && EF.Functions.ILike(x.NomeDepartamento, $"%{departamento}%"));
 if (d != null) user.DepartamentoId = d.DepartamentoId;
 }
 await _db.SaveChangesAsync();
 return RedirectToAction("Page", new { viewName = "GerenciarUsuarios" });
 }

 [HttpPost]
 [Authorize(Roles = "1")]
 [Route("Site/ReprovarUsuario")]
 [Route("reprovar-usuario")]
 public async Task<IActionResult> ReprovarUsuario(int solicitacao_id)
 {
 var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == solicitacao_id);
 if (user == null) return NotFound();
 // here we delete the user (or mark as rejected) - we'll delete
 _db.Usuarios.Remove(user);
 await _db.SaveChangesAsync();
 return RedirectToAction("Page", new { viewName = "GerenciarUsuarios" });
 }

 // Admin: Gerenciar Usuarios (GET)
 [HttpGet]
 [Authorize(Roles = "1")]
 [Route("Site/GerenciarUsuarios")]
 public async Task<IActionResult> GerenciarUsuarios(string? busca = null)
 {
 var query = _db.Usuarios
 .Include(u => u.Departamento)
 .Include(u => u.PerfilUsuario)
 .AsQueryable();

 if (!string.IsNullOrWhiteSpace(busca))
 {
 var term = busca.Trim();
 query = query.Where(u =>
 (u.NomeCompleto != null && EF.Functions.ILike(u.NomeCompleto, $"%{term}%")) ||
 (u.Email != null && EF.Functions.ILike(u.Email, $"%{term}%"))
 );
 }

 var users = await query.OrderBy(u => u.UsuarioId).ToListAsync();

 var model = new PIM_FINAL.Models.AdminUsersViewModel
 {
 ActiveUsers = users.Where(u => u.PerfilId != null).ToList(),
 PendingUsers = users.Where(u => u.PerfilId == null).ToList()
 };

 // carregar listas para selects (perfil/departamento)
 ViewBag.Perfis = await _db.PerfisUsuario.OrderBy(p => p.NomePerfil).ToListAsync();
 ViewBag.Departamentos = await _db.Departamentos.OrderBy(d => d.NomeDepartamento).ToListAsync();
 ViewBag.Busca = busca;

 return View("~/Views/site/GerenciarUsuarios.cshtml", model);
 }

 [HttpGet]
 [Authorize]
 [Route("Site/DownloadAnexo/{id}")]
 public IActionResult DownloadAnexo(int id)
 {
 var anexo = _db.Anexos.Find(id);
 if (anexo == null || string.IsNullOrEmpty(anexo.CaminhoArquivo)) return NotFound();
 var relative = anexo.CaminhoArquivo.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
 var physical = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relative);
 if (!System.IO.File.Exists(physical)) return NotFound();
 var contentType = anexo.TipoArquivo ?? "application/octet-stream";
 return PhysicalFile(physical, contentType, anexo.NomeArquivo);
 }

 // Editar usuário (admin)
 [HttpGet]
 [Authorize(Roles = "1")]
 [Route("Site/EditarUsuario/{id}")]
 public async Task<IActionResult> EditarUsuario(int id)
 {
 var user = await _db.Usuarios
 .Include(u => u.Departamento)
 .Include(u => u.PerfilUsuario)
 .FirstOrDefaultAsync(u => u.UsuarioId == id);
 if (user == null) return NotFound();

 var perfis = await _db.PerfisUsuario.OrderBy(p => p.NomePerfil).ToListAsync();
 var deps = await _db.Departamentos.OrderBy(d => d.NomeDepartamento).ToListAsync();

 var vm = new PIM_FINAL.Models.EditUsuarioViewModel
 {
 UsuarioId = user.UsuarioId,
 NomeCompleto = user.NomeCompleto,
 Email = user.Email,
 PerfilId = user.PerfilId,
 DepartamentoId = user.DepartamentoId,
 Perfis = perfis.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(p.NomePerfil, p.PerfilId.ToString())).ToList(),
 Departamentos = deps.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(d.NomeDepartamento, d.DepartamentoId.ToString())).ToList()
 };

 return View("~/Views/site/EditarUsuario.cshtml", vm);
 }

 [HttpPost]
 [Authorize(Roles = "1")]
 [Route("Site/EditarUsuario/{id}")]
 public async Task<IActionResult> EditarUsuario(int id, PIM_FINAL.Models.EditUsuarioViewModel vm, string? returnUrl = null)
 {
 if (id != vm.UsuarioId) return BadRequest();
 if (!ModelState.IsValid)
 {
 // reload selects
 vm.Perfis = (await _db.PerfisUsuario.OrderBy(p => p.NomePerfil).ToListAsync()).Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(p.NomePerfil, p.PerfilId.ToString())).ToList();
 vm.Departamentos = (await _db.Departamentos.OrderBy(d => d.NomeDepartamento).ToListAsync()).Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(d.NomeDepartamento, d.DepartamentoId.ToString())).ToList();
 return View("~/Views/site/EditarUsuario.cshtml", vm);
 }

 var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == id);
 if (user == null) return NotFound();

 user.NomeCompleto = vm.NomeCompleto;
 user.Email = vm.Email;
 user.PerfilId = vm.PerfilId;
 user.DepartamentoId = vm.DepartamentoId;

 if (!string.IsNullOrWhiteSpace(vm.NewPassword))
 {
 var hasher = new PasswordHasher<Usuario>();
 user.Senha = hasher.HashPassword(user, vm.NewPassword);
 }

 await _db.SaveChangesAsync();
 TempData["SuccessMessage"] = "Usuário atualizado com sucesso.";

 if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
 {
 return Redirect(returnUrl);
 }
 return RedirectToAction("GerenciarUsuarios");
 }

 [HttpGet]
 [Authorize]
 [Route("Site/InicialPainel")]
 public async Task<IActionResult> InicialPainel()
 {
 var vm = new InicialPainelViewModel();
 var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
 if (int.TryParse(userIdClaim, out var userId))
 {
 var user = await _db.Usuarios.Include(u => u.Departamento).Include(u => u.PerfilUsuario).FirstOrDefaultAsync(u => u.UsuarioId == userId);
 vm.NomeUsuario = user?.NomeCompleto ?? User?.Identity?.Name;
 vm.Perfil = user?.PerfilUsuario?.NomePerfil ?? (User?.FindFirst(ClaimTypes.Role)?.Value ?? "");
 }
 else
 {
 vm.NomeUsuario = User?.Identity?.Name ?? "(anônimo)";
 vm.Perfil = "(não autenticado)";
 }

 // Recent notifications: include latest Comunicacoes and Anexos for chamados owned by the user
 int uid =0;
 int.TryParse(userIdClaim, out uid);
 var notifications = new List<(DateTime When, string Text)>();
 if (uid >0)
 {
 var comms = await _db.Comunicacoes
 .Where(c => c.Chamado.UsuarioSolicitanteId == uid || c.UsuarioId == uid)
 .Include(c => c.Chamado)
 .OrderByDescending(c => c.DataEnvio)
 .Take(10)
 .ToListAsync();
 foreach (var c in comms)
 {
 var when = c.DataEnvio ?? DateTime.UtcNow;
 var proto = c.Chamado?.Protocolo ?? "#?";
 notifications.Add((when, $"[{proto}] {c.Mensagem}"));
 }

 var anexos = await _db.Anexos
 .Where(a => a.Chamado.UsuarioSolicitanteId == uid || a.UsuarioId == uid)
 .Include(a => a.Chamado)
 .OrderByDescending(a => a.DataUpload)
 .Take(10)
 .ToListAsync();
 foreach (var a in anexos)
 {
 var when = a.DataUpload ?? DateTime.UtcNow;
 var proto = a.Chamado?.Protocolo ?? "#?";
 notifications.Add((when, $"[{proto}] Anexo enviado: {a.NomeArquivo}"));
 }
 }

 vm.RecentNotifications = notifications
 .OrderByDescending(n => n.When)
 .Take(5)
 .Select(n => n.Text)
 .ToList();
 if (!vm.RecentNotifications.Any()) vm.RecentNotifications.Add("Nenhuma notificação recente.");

 // KPIs
 vm.ChamadosEmEspera = await _db.Chamados.CountAsync(c => c.StatusId == null || (c.StatusChamado != null && EF.Functions.ILike(c.StatusChamado.NomeStatus, "%abert%")));
 vm.ChamadosEmAtendimento = await _db.Chamados.CountAsync(c => c.StatusChamado != null && EF.Functions.ILike(c.StatusChamado.NomeStatus, "%atend%"));
 var hojeStart = DateTime.UtcNow.Date;
 var hojeEnd = hojeStart.AddDays(1);
 vm.ResolvidosHoje = await _db.Chamados.CountAsync(c => c.DataFechamento != null && c.DataFechamento >= hojeStart && c.DataFechamento < hojeEnd);
 var total = await _db.Chamados.CountAsync();
 // Use sla_atingido column as SLA met indicator
 var slaMet = await _db.Chamados.CountAsync(c => c.SlaAtingido == true);
 vm.SLACumpridoPercent = total ==0 ?100 : (int)Math.Round((double)slaMet / Math.Max(1, total) *100);

 return View("~/Views/site/InicialPainel.cshtml", vm);
 }

 // Utility: build CSV FileContentResult
 private FileContentResult CsvDownload(string fileName, IEnumerable<string> lines)
 {
 var sb = new StringBuilder();
 foreach (var l in lines) sb.AppendLine(l);
 var bytes = Encoding.UTF8.GetBytes(sb.ToString());
 return File(bytes, "text/csv; charset=utf-8", fileName);
 }

 // Dashboard export (called by a form with action "/exportar-dashboard")
 [HttpPost]
 [Authorize(Roles = "1,2")]
 [Route("exportar-dashboard")]
 public async Task<IActionResult> ExportarDashboard()
 {
 var total = await _db.Chamados.CountAsync();
 var resolvidos = await _db.Chamados.CountAsync(c => c.DataFechamento != null);
 var taxa = total ==0 ?100.0 : Math.Round(resolvidos *100.0 / total,1);
 var sla = total ==0 ?100 : (int)Math.Round((await _db.Chamados.CountAsync(c => c.SlaAtingido == true)) *100.0 / Math.Max(1, total));
 var linhas = new List<string>
 {
 "KPI,Valor",
 $"TotalChamados,{total}",
 $"ChamadosResolvidos,{resolvidos}",
 $"TaxaResolucaoPercent,{taxa}",
 $"SLACumpridoPercent,{sla}"
 };
 return CsvDownload($"dashboard_{DateTime.UtcNow:yyyyMMddHHmmss}.csv", linhas);
 }

 // Relatórios: aceitar POST das páginas que usam handler via query (?handler=GerarRelatorio / ?handler=ExportarCompleto)
 [HttpPost]
 [Authorize(Roles = "1,2")]
 [Route("Site/Relatorios")]
 public async Task<IActionResult> RelatoriosPost([FromQuery] string? handler, string? periodo, string? formato, string? tipo_relatorio)
 {
 handler = handler?.ToLowerInvariant();
 if (handler == "exportarcompleto")
 {
 // Exportar conjunto simples de dados (CSV) como prova de conceito
 var chamados = await _db.Chamados.Include(c => c.Prioridade).Include(c => c.StatusChamado).ToListAsync();
 var linhas = new List<string> { "Protocolo,Titulo,Status,Prioridade,DataAbertura,DataFechamento" };
 foreach (var c in chamados)
 {
 linhas.Add($"{c.Protocolo},{c.Titulo.Replace(',', ';')},{c.StatusChamado?.NomeStatus},{c.Prioridade?.NomePrioridade},{c.DataAbertura:O},{c.DataFechamento:O}");
 }
 return CsvDownload($"relatorios_completo_{DateTime.UtcNow:yyyyMMddHHmmss}.csv", linhas);
 }
 if (handler == "gerarrelatorio")
 {
 // Geração simples conforme período (CSV)
 DateTime inicio = DateTime.UtcNow.Date;
 if (string.Equals(periodo, "semana", StringComparison.OrdinalIgnoreCase)) inicio = inicio.AddDays(-7);
 else if (string.Equals(periodo, "mes", StringComparison.OrdinalIgnoreCase)) inicio = inicio.AddDays(-30);
 var chamados = await _db.Chamados.Where(c => c.DataAbertura >= inicio).Include(c => c.Prioridade).Include(c => c.StatusChamado).ToListAsync();
 var linhas = new List<string> { "Protocolo,Titulo,Status,Prioridade,DataAbertura" };
 foreach (var c in chamados)
 {
 linhas.Add($"{c.Protocolo},{c.Titulo.Replace(',', ';')},{c.StatusChamado?.NomeStatus},{c.Prioridade?.NomePrioridade},{c.DataAbertura:O}");
 }
 return CsvDownload($"relatorio_{periodo ?? "hoje"}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv", linhas);
 }
 // fallback: voltar para view
 return RedirectToAction("Relatorios");
 }

 // Enhanced Dashboard with avg resolution time and team performance
 [HttpGet]
 [Authorize(Roles = "1,2")]
 [Route("Site/DashboardGestor")]
 public async Task<IActionResult> DashboardGestor(string? periodo = "hoje")
 {
 var vm = new DashboardGestorViewModel();
 DateTime fim = DateTime.UtcNow; // fim do intervalo (agora)
 DateTime inicio = fim.Date; // padrão: hoje (00:00)
 string pNorm = (periodo ?? "hoje").ToLowerInvariant();
 switch (pNorm)
 {
 case "semana": inicio = inicio.AddDays(-7); break;
 case "mes": inicio = inicio.AddDays(-30); break;
 }
 ViewBag.Periodo = pNorm;
 ViewBag.PeriodoDescricao = pNorm == "hoje" ? "Hoje" : (pNorm == "semana" ? "Últimos7 dias" : (pNorm == "mes" ? "Últimos30 dias" : "Hoje"));
 ViewBag.PeriodoInicio = inicio;
 ViewBag.PeriodoFim = fim;

 var queryPeriodo = _db.Chamados.Where(c => c.DataAbertura != null && c.DataAbertura >= inicio && c.DataAbertura <= fim);
 vm.TotalChamados = await queryPeriodo.CountAsync();
 vm.ChamadosResolvidos = await _db.Chamados.CountAsync(c => c.DataFechamento != null && c.DataFechamento >= inicio && c.DataFechamento <= fim);
 vm.TaxaResolucaoPercent = vm.TotalChamados ==0 ?100 : Math.Round((double)vm.ChamadosResolvidos / Math.Max(1, vm.TotalChamados) *100,1);
 vm.SLACumpridoPercent = vm.TotalChamados ==0 ?100 : (int)Math.Round((double)(await queryPeriodo.CountAsync(c => c.SlaAtingido == true)) / Math.Max(1, vm.TotalChamados) *100);
 var avals = await _db.Avaliacoes.Where(a => a.DataAvaliacao >= inicio && a.DataAvaliacao <= fim).ToListAsync();
 if (avals.Any()) { vm.SatisfacaoMedia = Math.Round(avals.Average(a => a.Nota),2); vm.TotalAvaliacoes = avals.Count; }
 var resolvidosPeriodo = await _db.Chamados.Where(c => c.DataAbertura != null && c.DataAbertura >= inicio && c.DataAbertura <= fim && c.DataFechamento != null).Select(c => new { c.DataAbertura, c.DataFechamento }).ToListAsync();
 if (resolvidosPeriodo.Any())
 {
 var avgSeconds = resolvidosPeriodo.Average(c => (c.DataFechamento!.Value - c.DataAbertura!.Value).TotalSeconds);
 var ts = TimeSpan.FromSeconds(avgSeconds);
 vm.TempoMedioResolucao = ts.TotalHours >=1 ? $"{(int)ts.TotalHours}h{ts.Minutes}m" : $"{ts.Minutes}m";
 }
 var tecnicos = await _db.Usuarios.Where(u => u.PerfilId ==1 || u.PerfilId ==2 || u.PerfilId ==3).ToListAsync();
 foreach (var t in tecnicos)
 {
 var atendidos = await queryPeriodo.CountAsync(c => c.TecnicoResponsavelId == t.UsuarioId);
 var resolv = await _db.Chamados.CountAsync(c => c.TecnicoResponsavelId == t.UsuarioId && c.DataFechamento != null && c.DataFechamento >= inicio && c.DataFechamento <= fim);
 var taxa = atendidos ==0 ?0 : Math.Round(resolv *100.0 / atendidos,1);
 vm.DesempenhoEquipe.Add((t.NomeCompleto ?? "-", atendidos, resolv, taxa, "-",0));
 }
 return View("~/Views/site/DashboardGestor.cshtml", vm);
 }

 // Aliases for "from Views" links
 [HttpGet]
 [Authorize]
 [Route("Site/AbrirChamado_FromViews")]
 public IActionResult AbrirChamado_FromViews() => RedirectToAction("AbrirChamado");

 [HttpGet]
 [Authorize]
 [Route("Site/Escalonamento_FromViews")]
 public IActionResult Escalonamento_FromViews() => RedirectToAction("Escalonamento");

 [HttpGet]
 [Authorize(Roles = "1,2")]
 [Route("Site/DashboardGestor_FromViews")]
 public IActionResult DashboardGestor_FromViews() => RedirectToAction("DashboardGestor");

 [HttpGet]
 [Authorize]
 [Route("Site/DetalhesEscalonamento_FromViews/{id}")]
 public IActionResult DetalhesEscalonamento_FromViews(int id) => RedirectToAction("DetalhesEscalonamento", new { id });

 // Public GET views (Login/Cadastro/Recuperar/Validar)
 [HttpGet]
 [AllowAnonymous]
 [Route("Site/Login")]
 public IActionResult LoginView() => View("~/Views/site/Login.cshtml");

 [HttpGet]
 [AllowAnonymous]
 [Route("Site/Cadastro")]
 public IActionResult CadastroView() => View("~/Views/site/Cadastro.cshtml");

 [HttpGet]
 [AllowAnonymous]
 [Route("Site/RecuperarSenha")]
 public IActionResult RecuperarSenhaView() => View("~/Views/site/RecuperarSenha.cshtml");

 [HttpGet]
 [AllowAnonymous]
 [Route("Site/ValidarCodigoRedefinir")]
 public IActionResult ValidarCodigoRedefinirView(string? email = null)
 {
 ViewBag.ResetEmail = email; // expose email used in recovery
 return View("~/Views/site/ValidarCodigoRedefinir.cshtml");
 }

 // Detalhes Escalonamento (GET)
 [HttpGet]
 [Authorize]
 [Route("Site/DetalhesEscalonamento/{id}")]
 public async Task<IActionResult> DetalhesEscalonamento(int id)
 {
 var chamado = await _db.Chamados
 .Include(c => c.Prioridade)
 .Include(c => c.StatusChamado)
 .Include(c => c.Categoria)
 .Include(c => c.Sla)
 .Include(c => c.Comunicacoes).ThenInclude(cm => cm.Usuario)
 .FirstOrDefaultAsync(c => c.ChamadoId == id);
 if (chamado == null) return NotFound();

 ViewBag.chamadoId = chamado.ChamadoId;
 ViewBag.protocolo = chamado.Protocolo;
 // compute SLA consumption/remaining
 if (chamado.Sla != null && chamado.DataAbertura.HasValue && !chamado.DataFechamento.HasValue)
 {
 var elapsed = DateTime.UtcNow - chamado.DataAbertura.Value;
 var slaResolution = TimeSpan.FromHours(chamado.Sla.TempoMaximoResolucao);
 var consumed = (int)Math.Min(100, (elapsed.TotalSeconds / Math.Max(1, slaResolution.TotalSeconds)) *100);
 var remaining = slaResolution - elapsed;
 ViewBag.SlaConsumed = consumed;
 ViewBag.SlaRemaining = remaining > TimeSpan.Zero ? $"{(int)remaining.TotalHours}h{remaining.Minutes}min" : "0min";
 }

 return View("~/Views/site/DetalhesEscalonamento.cshtml", chamado);
 }

 // Fix Escalonamento GET duplicate signature: keep one with optional filters
 [HttpGet]
 [Authorize]
 [Route("Site/Escalonamento")]
 public async Task<IActionResult> Escalonamento(string? urgencia = null, string? protocolo = null)
 {
 var role = User.FindFirst(ClaimTypes.Role)?.Value;
 bool isTecnico = role == "3" || role == "2" || role == "1";
 if (!isTecnico) return Forbid();

 // Base query with includes
 var baseQuery = _db.Chamados
 .Include(c => c.Prioridade)
 .Include(c => c.TecnicoResponsavel)
 .Include(c => c.Sla)
 .Include(c => c.Atendimentos)
 .AsQueryable();

 IQueryable<Chamado> q;
 bool isSearch = !string.IsNullOrWhiteSpace(protocolo);
 if (isSearch)
 {
 var proto = protocolo!.Trim();
 var proto2 = proto.TrimStart('#');
 q = baseQuery.Where(c => c.Protocolo != null && (
 EF.Functions.ILike(c.Protocolo, $"%{proto}%") || EF.Functions.ILike(c.Protocolo, $"%{proto2}%")
));
 }
 else
 {
 // default list: only abertos (em risco)
 q = baseQuery.Where(c => c.DataAbertura != null && c.DataFechamento == null);
 }

 var chamados = await q.ToListAsync();

 var list = new List<ChamadoSummary>();
 foreach (var c in chamados)
 {
 var sla = c.Sla;
 TimeSpan? remaining = null;
 int consumed =0;
 if (sla != null && c.DataAbertura.HasValue)
 {
 var end = c.DataFechamento ?? DateTime.UtcNow;
 var elapsed = end - c.DataAbertura.Value;
 var slaResolution = TimeSpan.FromHours(sla.TempoMaximoResolucao);
 consumed = (int)Math.Min(100, (elapsed.TotalSeconds / Math.Max(1, slaResolution.TotalSeconds)) *100);
 // only show remaining for abertos
 if (!c.DataFechamento.HasValue)
 {
 var rem = slaResolution - elapsed;
 remaining = rem > TimeSpan.Zero ? rem : TimeSpan.Zero;
 }
 }

 bool include = true;
 if (!isSearch)
 {
 // default view: risco por SLA consumo
 include = consumed >=50;
 if (!string.IsNullOrEmpty(urgencia))
 {
 if (urgencia.Equals("sla_75", StringComparison.OrdinalIgnoreCase)) include = consumed >=75;
 else if (urgencia.Equals("sla_50", StringComparison.OrdinalIgnoreCase)) include = consumed >=50 && consumed <75;
 else if (urgencia.Equals("critico", StringComparison.OrdinalIgnoreCase))
 {
 var isCritica = c.Prioridade?.NomePrioridade != null && NormalizeKey(c.Prioridade.NomePrioridade).Contains("critica");
 var semResposta = c.Atendimentos == null || !c.Atendimentos.Any();
 include = isCritica && semResposta;
 }
 }
 }

 if (!include) continue;

 list.Add(new ChamadoSummary
 {
 ChamadoId = c.ChamadoId,
 Protocolo = c.Protocolo,
 Titulo = c.Titulo,
 Prioridade = c.Prioridade?.NomePrioridade,
 Tecnico = c.TecnicoResponsavel?.NomeCompleto ?? "(sem técnico)",
 SlaConsumedPercent = consumed,
 TimeRemaining = remaining
 });
 }

 // For search, keep natural order by protocolo; otherwise order by SLA desc
 list = isSearch
 ? list.OrderBy(i => i.Protocolo).ToList()
 : list.OrderByDescending(i => i.SlaConsumedPercent).ThenBy(i => i.Protocolo).ToList();

 return View("~/Views/site/Escalonamento.cshtml", list);
 }

 // Restore missing actions (Configuracao, Relatorios GET, AprovarUsuario GET)
 // GET Configuração (admin)
 [HttpGet]
 [Authorize(Roles = "1")]
 [Route("Site/Configuracao")]
 public async Task<IActionResult> Configuracao()
 {
 var deps = await _db.Departamentos.OrderBy(d => d.NomeDepartamento).ToListAsync();
 var perfis = await _db.PerfisUsuario.OrderBy(p => p.NomePerfil).ToListAsync();
 ViewBag.Departamentos = deps;
 ViewBag.Perfis = perfis;
 return View("~/Views/site/Configuracao.cshtml");
 }

 // GET AprovarUsuario page (admin) shows pending users (PerfilId null)
 [HttpGet]
 [Authorize(Roles = "1")]
 [Route("Site/AprovarUsuario")]
 public async Task<IActionResult> AprovarUsuarioView()
 {
 var pendentes = await _db.Usuarios.Where(u => u.PerfilId == null).OrderBy(u => u.UsuarioId).ToListAsync();
 // carregar opções para selects
 ViewBag.Departamentos = await _db.Departamentos.OrderBy(d => d.NomeDepartamento).ToListAsync();
 ViewBag.Perfis = await _db.PerfisUsuario.OrderBy(p => p.NomePerfil).ToListAsync();
 return View("~/Views/site/AprovarUsuario.cshtml", pendentes);
 }

 // GETRelatorios (build model)
 [HttpGet]
 [Authorize(Roles = "1,2")]
 [Route("Site/Relatorios")]
 public async Task<IActionResult> Relatorios(string? periodo = "hoje")
 {
 var vm = await BuildRelatoriosViewModel(periodo);
 return View("~/Views/site/Relatorios.cshtml", vm);
 }

 private async Task<RelatoriosViewModel> BuildRelatoriosViewModel(string? periodo)
 {
 DateTime fim = DateTime.UtcNow;
 DateTime inicio = fim.Date;
 if (string.Equals(periodo, "semana", StringComparison.OrdinalIgnoreCase)) inicio = inicio.AddDays(-7);
 else if (string.Equals(periodo, "mes", StringComparison.OrdinalIgnoreCase)) inicio = inicio.AddDays(-30);
 var vm = new RelatoriosViewModel { PeriodoInicio = inicio, PeriodoFim = fim };
 var chamadosPeriodo = await _db.Chamados
 .Include(c => c.StatusChamado)
 .Include(c => c.Prioridade)
 .Include(c => c.Categoria)
 .Include(c => c.UsuarioSolicitante).ThenInclude(u => u.Departamento)
 .Where(c => c.DataAbertura >= inicio && c.DataAbertura <= fim)
 .ToListAsync();
 vm.TotalChamadosPeriodo = chamadosPeriodo.Count;
 vm.TotalResolvidosPeriodo = chamadosPeriodo.Count(c => c.DataFechamento != null);
 vm.TaxaResolucaoPercent = vm.TotalChamadosPeriodo ==0 ?100 : Math.Round(vm.TotalResolvidosPeriodo *100.0 / vm.TotalChamadosPeriodo,1);
 var resolvidos = chamadosPeriodo.Where(c => c.DataFechamento != null).ToList();
 if (resolvidos.Any()) vm.TempoMedioResolucao = Math.Round(resolvidos.Average(c => (c.DataFechamento.Value - c.DataAbertura.Value).TotalHours),1) + "h";
 foreach (var grp in chamadosPeriodo.GroupBy(c => c.StatusChamado?.NomeStatus ?? "(sem status)"))
 {
 var pct = vm.TotalChamadosPeriodo ==0 ?0 : Math.Round(grp.Count() *100.0 / vm.TotalChamadosPeriodo,1);
 vm.Status.Add(new ItemResumo { Nome = grp.Key, Quantidade = grp.Count(), Percentual = pct });
 }
 foreach (var grp in chamadosPeriodo.GroupBy(c => c.Prioridade?.NomePrioridade ?? "(sem)"))
 {
 var pct = vm.TotalChamadosPeriodo ==0 ?0 : Math.Round(grp.Count() *100.0 / vm.TotalChamadosPeriodo,1);
 vm.Prioridades.Add(new ItemResumo { Nome = grp.Key, Quantidade = grp.Count(), Percentual = pct });
 }
 foreach (var grp in chamadosPeriodo.GroupBy(c => c.Categoria?.NomeCategoria ?? "(sem)"))
 {
 var pct = vm.TotalChamadosPeriodo ==0 ?0 : Math.Round(grp.Count() *100.0 / vm.TotalChamadosPeriodo,1);
 vm.Categorias.Add(new ItemResumo { Nome = grp.Key, Quantidade = grp.Count(), Percentual = pct });
 }
 foreach (var grp in chamadosPeriodo.GroupBy(c => c.UsuarioSolicitante?.Departamento?.NomeDepartamento ?? "(sem)"))
 {
 var pct = vm.TotalChamadosPeriodo ==0 ?0 : Math.Round(grp.Count() *100.0 / vm.TotalChamadosPeriodo,1);
 vm.Departamentos.Add(new ItemResumo { Nome = grp.Key, Quantidade = grp.Count(), Percentual = pct });
 }
 var tecnicosIds = chamadosPeriodo.Where(c => c.TecnicoResponsavelId != null).Select(c => c.TecnicoResponsavelId!.Value).Distinct().ToList();
 var tecnicos = await _db.Usuarios.Where(u => tecnicosIds.Contains(u.UsuarioId)).ToListAsync();
 // cria queryPeriodo local para reutilização
 var queryPeriodo = _db.Chamados.Where(c => c.DataAbertura >= inicio && c.DataAbertura <= fim);
 foreach (var t in tecnicos)
 {
 var atendidos = await queryPeriodo.CountAsync(c => c.TecnicoResponsavelId == t.UsuarioId);
 var resolvT = await _db.Chamados.CountAsync(c => c.TecnicoResponsavelId == t.UsuarioId && c.DataFechamento != null && c.DataFechamento >= inicio && c.DataFechamento <= fim);
 var taxa = atendidos ==0 ?0 : Math.Round(resolvT *100.0 / atendidos,1);
 vm.Tecnicos.Add(new TecnicoResumo { Nome = t.NomeCompleto ?? "-", Atendidos = atendidos, Resolvidos = resolvT, TaxaResolucao = taxa });
 }
 foreach (var grp in chamadosPeriodo.GroupBy(c => c.Prioridade?.NomePrioridade ?? "(sem)"))
 {
 int total = grp.Count();
 int cumpridos = grp.Count(c => c.SlaAtingido == true);
 int violados = total - cumpridos;
 double taxa = total ==0 ?0 : Math.Round(cumpridos *100.0 / total,1);
 vm.SlaPorPrioridade.Add(new SlaResumo { Prioridade = grp.Key, Total = total, Cumpridos = cumpridos, Violados = violados, TaxaCumprimento = taxa });
 }
 var avalsPeriodo = await _db.Avaliacoes.Where(a => a.DataAvaliacao >= inicio && a.DataAvaliacao <= fim).ToListAsync();
 vm.TotalAvaliacoes = avalsPeriodo.Count;
 if (avalsPeriodo.Any()) vm.SatisfacaoMedia = Math.Round(avalsPeriodo.Average(a => a.Nota),2);
 for (int nota =1; nota <=5; nota++)
 {
 int qtd = avalsPeriodo.Count(a => a.Nota == nota);
 double pct = vm.TotalAvaliacoes ==0 ?0 : Math.Round(qtd *100.0 / vm.TotalAvaliacoes,1);
 vm.DistribuicaoNotas.Add(new ItemResumo { Nome = nota + " estrela" + (nota >1 ? "s" : ""), Quantidade = qtd, Percentual = pct });
 }
 return vm;
 }

 // Recuperar senha (POST gera token fictício)
 [HttpPost]
 [AllowAnonymous]
 [Route("Site/RecuperarSenha")]
 public async Task<IActionResult> RecuperarSenha(string email)
 {
 var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
 if (user == null) { TempData["ErrorMessage"] = "E-mail não encontrado"; return RedirectToAction("RecuperarSenhaView"); }
 // Sem servidor de e-mail: pular envio de código e ir direto para tela de redefinição
 TempData["InfoMessage"] = "Defina uma nova senha abaixo.";
 return RedirectToAction("ValidarCodigoRedefinirView", new { email });
 }

 // Removido parâmetro de código para redefinição direta
 [HttpPost]
 [AllowAnonymous]
 [Route("Site/ValidarCodigoRedefinir")]
 public async Task<IActionResult> ValidarCodigoRedefinir(string email, string novaSenha, string confirmaSenha)
 {
 if (string.IsNullOrWhiteSpace(email)) { TempData["ErrorMessage"] = "E-mail inválido"; return RedirectToAction("RecuperarSenhaView"); }
 if (string.IsNullOrWhiteSpace(novaSenha) || string.IsNullOrWhiteSpace(confirmaSenha)) { TempData["ErrorMessage"] = "Preenchas as senhas"; return RedirectToAction("ValidarCodigoRedefinirView", new { email }); }
 if (novaSenha != confirmaSenha) { TempData["ErrorMessage"] = "Senhas não coincidem"; return RedirectToAction("ValidarCodigoRedefinirView", new { email }); }
 var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
 if (user == null) { TempData["ErrorMessage"] = "Usuário não encontrado"; return RedirectToAction("RecuperarSenhaView"); }
 var hasher = new PasswordHasher<Usuario>();
 user.Senha = hasher.HashPassword(user, novaSenha);
 await _db.SaveChangesAsync();
 TempData["SuccessMessage"] = "Senha redefinida com sucesso";
 return RedirectToAction("LoginView");
 }

 // Email util
 private async Task SendNotificationEmailAsync(string? to, string subject, string body)
 {
 if (string.IsNullOrWhiteSpace(to)) return;
 var smtpHost = _config["Smtp:Host"]; if (string.IsNullOrWhiteSpace(smtpHost)) return;
 try
 {
 var port = int.TryParse(_config["Smtp:Port"], out var p) ? p :25;
 var user = _config["Smtp:User"]; var pass = _config["Smtp:Pass"]; using var msg = new MailMessage();
 msg.To.Add(new MailAddress(to)); msg.From = new MailAddress(_config["Smtp:From"] ?? "noreply@example.com"); msg.Subject = subject; msg.Body = body;
 using var client = new SmtpClient(smtpHost, port); if (!string.IsNullOrEmpty(user)) client.Credentials = new System.Net.NetworkCredential(user, pass); client.EnableSsl = bool.TryParse(_config["Smtp:EnableSsl"], out var ssl) && ssl; await client.SendMailAsync(msg);
 }
 catch { }
 }

 private static string NormalizeKey(string? s)
 {
 if (string.IsNullOrWhiteSpace(s)) return string.Empty;
 var formD = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
 var sb = new StringBuilder(formD.Length);
 foreach (var ch in formD)
 {
 var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
 if (uc != UnicodeCategory.NonSpacingMark)
 sb.Append(ch);
 }
 return sb.ToString().Normalize(NormalizationForm.FormC);
 }
 }
}
