using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class HistoricoChamadoController : Controller
    {
        private readonly PIMContext _context;
        public HistoricoChamadoController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.HistoricosChamado.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var historico = await _context.HistoricosChamado.FirstOrDefaultAsync(m => m.HistoricoId == id);
            if (historico == null) return NotFound();
            return View(historico);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HistoricoId,ChamadoId,UsuarioId,Acao,Descricao,DataAcao,StatusAnteriorId,StatusNovoId")] HistoricoChamado historico)
        {
            if (ModelState.IsValid)
            {
                _context.Add(historico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(historico);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var historico = await _context.HistoricosChamado.FindAsync(id);
            if (historico == null) return NotFound();
            return View(historico);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HistoricoId,ChamadoId,UsuarioId,Acao,Descricao,DataAcao,StatusAnteriorId,StatusNovoId")] HistoricoChamado historico)
        {
            if (id != historico.HistoricoId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(historico);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.HistoricosChamado.Any(e => e.HistoricoId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(historico);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var historico = await _context.HistoricosChamado.FirstOrDefaultAsync(m => m.HistoricoId == id);
            if (historico == null) return NotFound();
            return View(historico);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var historico = await _context.HistoricosChamado.FindAsync(id);
            if (historico != null)
            {
                _context.HistoricosChamado.Remove(historico);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}