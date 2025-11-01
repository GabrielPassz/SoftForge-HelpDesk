using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class IAAnaliseController : Controller
    {
        private readonly PIMContext _context;
        public IAAnaliseController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.IAAnalises.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var iaAnalise = await _context.IAAnalises.FirstOrDefaultAsync(m => m.IAId == id);
            if (iaAnalise == null) return NotFound();
            return View(iaAnalise);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IAId,ChamadoId,CategoriaPrevista,Confianca,ResultadoValidado,Comentario,DataAnalise")] IAAnalise iaAnalise)
        {
            if (ModelState.IsValid)
            {
                _context.Add(iaAnalise);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(iaAnalise);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var iaAnalise = await _context.IAAnalises.FindAsync(id);
            if (iaAnalise == null) return NotFound();
            return View(iaAnalise);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IAId,ChamadoId,CategoriaPrevista,Confianca,ResultadoValidado,Comentario,DataAnalise")] IAAnalise iaAnalise)
        {
            if (id != iaAnalise.IAId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(iaAnalise);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.IAAnalises.Any(e => e.IAId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(iaAnalise);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var iaAnalise = await _context.IAAnalises.FirstOrDefaultAsync(m => m.IAId == id);
            if (iaAnalise == null) return NotFound();
            return View(iaAnalise);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var iaAnalise = await _context.IAAnalises.FindAsync(id);
            if (iaAnalise != null)
            {
                _context.IAAnalises.Remove(iaAnalise);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}