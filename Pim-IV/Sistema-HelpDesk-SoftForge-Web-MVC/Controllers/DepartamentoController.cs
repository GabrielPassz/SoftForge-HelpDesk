using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class DepartamentoController : Controller
    {
        private readonly PIMContext _context;
        public DepartamentoController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.Departamentos.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var departamento = await _context.Departamentos.FirstOrDefaultAsync(m => m.DepartamentoId == id);
            if (departamento == null) return NotFound();
            return View(departamento);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartamentoId,NomeDepartamento,Descricao")] Departamento departamento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(departamento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(departamento);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento == null) return NotFound();
            return View(departamento);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartamentoId,NomeDepartamento,Descricao")] Departamento departamento)
        {
            if (id != departamento.DepartamentoId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(departamento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Departamentos.Any(e => e.DepartamentoId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(departamento);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var departamento = await _context.Departamentos.FirstOrDefaultAsync(m => m.DepartamentoId == id);
            if (departamento == null) return NotFound();
            return View(departamento);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var departamento = await _context.Departamentos.FindAsync(id);
            if (departamento != null)
            {
                _context.Departamentos.Remove(departamento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}