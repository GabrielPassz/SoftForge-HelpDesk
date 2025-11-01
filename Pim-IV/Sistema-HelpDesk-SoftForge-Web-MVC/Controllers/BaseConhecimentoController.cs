using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class BaseConhecimentoController : Controller
    {
        private readonly PIMContext _context;
        public BaseConhecimentoController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.BasesConhecimento.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var baseConhecimento = await _context.BasesConhecimento.FirstOrDefaultAsync(m => m.BaseId == id);
            if (baseConhecimento == null) return NotFound();
            return View(baseConhecimento);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BaseId,Titulo,Descricao,Solucao,DataCriacao,Aprovado,CategoriaId,UsuarioCriadorId")] BaseConhecimento baseConhecimento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(baseConhecimento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(baseConhecimento);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var baseConhecimento = await _context.BasesConhecimento.FindAsync(id);
            if (baseConhecimento == null) return NotFound();
            return View(baseConhecimento);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BaseId,Titulo,Descricao,Solucao,DataCriacao,Aprovado,CategoriaId,UsuarioCriadorId")] BaseConhecimento baseConhecimento)
        {
            if (id != baseConhecimento.BaseId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(baseConhecimento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BasesConhecimento.Any(e => e.BaseId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(baseConhecimento);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var baseConhecimento = await _context.BasesConhecimento.FirstOrDefaultAsync(m => m.BaseId == id);
            if (baseConhecimento == null) return NotFound();
            return View(baseConhecimento);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var baseConhecimento = await _context.BasesConhecimento.FindAsync(id);
            if (baseConhecimento != null)
            {
                _context.BasesConhecimento.Remove(baseConhecimento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}