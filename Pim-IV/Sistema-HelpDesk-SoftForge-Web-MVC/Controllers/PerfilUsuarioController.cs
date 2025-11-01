using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class PerfilUsuarioController : Controller
    {
        private readonly PIMContext _context;
        public PerfilUsuarioController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.PerfisUsuario.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var perfil = await _context.PerfisUsuario.FirstOrDefaultAsync(m => m.PerfilId == id);
            if (perfil == null) return NotFound();
            return View(perfil);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PerfilId,NomePerfil,Descricao")] PerfilUsuario perfil)
        {
            if (ModelState.IsValid)
            {
                _context.Add(perfil);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(perfil);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var perfil = await _context.PerfisUsuario.FindAsync(id);
            if (perfil == null) return NotFound();
            return View(perfil);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PerfilId,NomePerfil,Descricao")] PerfilUsuario perfil)
        {
            if (id != perfil.PerfilId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(perfil);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PerfisUsuario.Any(e => e.PerfilId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(perfil);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var perfil = await _context.PerfisUsuario.FirstOrDefaultAsync(m => m.PerfilId == id);
            if (perfil == null) return NotFound();
            return View(perfil);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var perfil = await _context.PerfisUsuario.FindAsync(id);
            if (perfil != null)
            {
                _context.PerfisUsuario.Remove(perfil);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}