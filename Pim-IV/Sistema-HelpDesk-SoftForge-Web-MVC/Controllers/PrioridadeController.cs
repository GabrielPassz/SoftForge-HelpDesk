using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class PrioridadeController : Controller
    {
        private readonly PIMContext _context;
        public PrioridadeController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.Prioridades.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var prioridade = await _context.Prioridades.FirstOrDefaultAsync(m => m.PrioridadeId == id);
            if (prioridade == null) return NotFound();
            return View(prioridade);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrioridadeId,NomePrioridade,Descricao")] Prioridade prioridade)
        {
            if (ModelState.IsValid)
            {
                _context.Add(prioridade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(prioridade);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var prioridade = await _context.Prioridades.FindAsync(id);
            if (prioridade == null) return NotFound();
            return View(prioridade);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PrioridadeId,NomePrioridade,Descricao")] Prioridade prioridade)
        {
            if (id != prioridade.PrioridadeId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prioridade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Prioridades.Any(e => e.PrioridadeId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(prioridade);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var prioridade = await _context.Prioridades.FirstOrDefaultAsync(m => m.PrioridadeId == id);
            if (prioridade == null) return NotFound();
            return View(prioridade);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prioridade = await _context.Prioridades.FindAsync(id);
            if (prioridade != null)
            {
                _context.Prioridades.Remove(prioridade);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}