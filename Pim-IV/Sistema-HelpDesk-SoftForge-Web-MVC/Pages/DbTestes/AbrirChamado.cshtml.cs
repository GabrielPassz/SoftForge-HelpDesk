using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PIM.Data;
using PIM.Models;

namespace PIM.Pages.DbTestes
{
 public class AbrirChamadoModel : PageModel
 {
 private readonly PIMContext _context;
 private readonly IWebHostEnvironment _env;
 public AbrirChamadoModel(PIMContext context, IWebHostEnvironment env) => (_context, _env) = (context, env);

 [BindProperty]
 public string Titulo { get; set; }
 [BindProperty]
 public string Descricao { get; set; }
 [BindProperty]
 public string Categoria { get; set; }
 [BindProperty]
 public string Prioridade { get; set; }
 [BindProperty]
 public IFormFile AnexoFile { get; set; }

 public bool Created { get; set; }
 public string CreatedProtocol { get; set; }
 public string SolicitanteNome { get; set; } = "Teste Usuario";
 public string SolicitanteDepartamento { get; set; } = "TI";

 public void OnGet() { }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();

 var chamado = new Chamado
 {
 Protocolo = "TEST-" + Guid.NewGuid().ToString().Substring(0,8),
 Titulo = Titulo,
 Descricao = Descricao,
 DataAbertura = DateTime.Now,
 UsuarioSolicitanteId = _context.Usuarios.Select(u => u.UsuarioId).FirstOrDefault(),
 // map simples: prioridade e categoria não vinculados a FK neste teste
 };
 _context.Chamados.Add(chamado);
 await _context.SaveChangesAsync();

 Created = true;
 CreatedProtocol = chamado.Protocolo;

 // handle file upload
 if (AnexoFile != null && AnexoFile.Length >0)
 {
 var uploads = Path.Combine(_env.WebRootPath, "uploads");
 if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
 var fileName = Path.GetFileName(AnexoFile.FileName);
 var filePath = Path.Combine(uploads, fileName);
 using (var stream = System.IO.File.Create(filePath))
 {
 await AnexoFile.CopyToAsync(stream);
 }
 // create Anexo record
 var anexo = new Anexo
 {
 ChamadoId = chamado.ChamadoId,
 NomeArquivo = fileName,
 TipoArquivo = AnexoFile.ContentType,
 CaminhoArquivo = "/uploads/" + fileName,
 UsuarioId = chamado.UsuarioSolicitanteId,
 DataUpload = DateTime.Now
 };
 _context.Anexos.Add(anexo);
 await _context.SaveChangesAsync();
 }

 return Page();
 }
 }
}