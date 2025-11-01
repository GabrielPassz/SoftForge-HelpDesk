using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class StatusChamadoController : Controller
    {
        private readonly PIMContext _context;
        public StatusChamadoController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.StatusChamados.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var status = await _context.StatusChamados.FirstOrDefaultAsync(m => m.StatusId == id);
            if (status == null) return NotFound();
            return View(status);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StatusId,NomeStatus,Descricao")] StatusChamado status)
        {
            if (ModelState.IsValid)
            {
                _context.Add(status);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(status);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var status = await _context.StatusChamados.FindAsync(id);
            if (status == null) return NotFound();
            return View(status);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StatusId,NomeStatus,Descricao")] StatusChamado status)
        {
            if (id != status.StatusId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(status);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.StatusChamados.Any(e => e.StatusId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(status);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var status = await _context.StatusChamados.FirstOrDefaultAsync(m => m.StatusId == id);
            if (status == null) return NotFound();
            return View(status);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var status = await _context.StatusChamados.FindAsync(id);
            if (status != null)
            {
                _context.StatusChamados.Remove(status);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}