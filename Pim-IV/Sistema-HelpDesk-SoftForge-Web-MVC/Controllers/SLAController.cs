using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class SLAController : Controller
    {
        private readonly PIMContext _context;
        public SLAController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.SLAs.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var sla = await _context.SLAs.FirstOrDefaultAsync(m => m.SLAId == id);
            if (sla == null) return NotFound();
            return View(sla);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SLAId,NomeSLA,TempoPrimeiraResposta,TempoMaximoResolucao,Descricao,PrioridadeId")] SLA sla)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sla);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sla);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var sla = await _context.SLAs.FindAsync(id);
            if (sla == null) return NotFound();
            return View(sla);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SLAId,NomeSLA,TempoPrimeiraResposta,TempoMaximoResolucao,Descricao,PrioridadeId")] SLA sla)
        {
            if (id != sla.SLAId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sla);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SLAs.Any(e => e.SLAId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sla);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var sla = await _context.SLAs.FirstOrDefaultAsync(m => m.SLAId == id);
            if (sla == null) return NotFound();
            return View(sla);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sla = await _context.SLAs.FindAsync(id);
            if (sla != null)
            {
                _context.SLAs.Remove(sla);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}