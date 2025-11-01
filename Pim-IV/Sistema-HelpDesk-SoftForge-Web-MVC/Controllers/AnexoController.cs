using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class AnexoController : Controller
    {
        private readonly PIMContext _context;
        public AnexoController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.Anexos.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var anexo = await _context.Anexos.FirstOrDefaultAsync(m => m.AnexoId == id);
            if (anexo == null) return NotFound();
            return View(anexo);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnexoId,ChamadoId,NomeArquivo,TipoArquivo,CaminhoArquivo,DataUpload,UsuarioId")] Anexo anexo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(anexo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(anexo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var anexo = await _context.Anexos.FindAsync(id);
            if (anexo == null) return NotFound();
            return View(anexo);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AnexoId,ChamadoId,NomeArquivo,TipoArquivo,CaminhoArquivo,DataUpload,UsuarioId")] Anexo anexo)
        {
            if (id != anexo.AnexoId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(anexo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Anexos.Any(e => e.AnexoId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(anexo);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var anexo = await _context.Anexos.FirstOrDefaultAsync(m => m.AnexoId == id);
            if (anexo == null) return NotFound();
            return View(anexo);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anexo = await _context.Anexos.FindAsync(id);
            if (anexo != null)
            {
                _context.Anexos.Remove(anexo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}