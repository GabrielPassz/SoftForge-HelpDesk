using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class AvaliacaoController : Controller
    {
        private readonly PIMContext _context;
        public AvaliacaoController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.Avaliacoes.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var avaliacao = await _context.Avaliacoes.FirstOrDefaultAsync(m => m.AvaliacaoId == id);
            if (avaliacao == null) return NotFound();
            return View(avaliacao);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AvaliacaoId,ChamadoId,UsuarioSolicitanteId,Nota,Comentario,DataAvaliacao")] Avaliacao avaliacao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(avaliacao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(avaliacao);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null) return NotFound();
            return View(avaliacao);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AvaliacaoId,ChamadoId,UsuarioSolicitanteId,Nota,Comentario,DataAvaliacao")] Avaliacao avaliacao)
        {
            if (id != avaliacao.AvaliacaoId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(avaliacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Avaliacoes.Any(e => e.AvaliacaoId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(avaliacao);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var avaliacao = await _context.Avaliacoes.FirstOrDefaultAsync(m => m.AvaliacaoId == id);
            if (avaliacao == null) return NotFound();
            return View(avaliacao);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao != null)
            {
                _context.Avaliacoes.Remove(avaliacao);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}