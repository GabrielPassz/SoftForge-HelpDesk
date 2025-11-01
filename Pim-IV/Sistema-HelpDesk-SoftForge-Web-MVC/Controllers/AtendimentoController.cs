using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class AtendimentoController : Controller
    {
        private readonly PIMContext _context;
        public AtendimentoController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.Atendimentos.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var atendimento = await _context.Atendimentos.FirstOrDefaultAsync(m => m.AtendimentoId == id);
            if (atendimento == null) return NotFound();
            return View(atendimento);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AtendimentoId,ChamadoId,UsuarioTecnicoId,DataAtendimento,AcaoRealizada,TempoGasto,SolucaoIA,SolucaoBaseConhecimentoId")] Atendimento atendimento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(atendimento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(atendimento);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var atendimento = await _context.Atendimentos.FindAsync(id);
            if (atendimento == null) return NotFound();
            return View(atendimento);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AtendimentoId,ChamadoId,UsuarioTecnicoId,DataAtendimento,AcaoRealizada,TempoGasto,SolucaoIA,SolucaoBaseConhecimentoId")] Atendimento atendimento)
        {
            if (id != atendimento.AtendimentoId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(atendimento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Atendimentos.Any(e => e.AtendimentoId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(atendimento);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var atendimento = await _context.Atendimentos.FirstOrDefaultAsync(m => m.AtendimentoId == id);
            if (atendimento == null) return NotFound();
            return View(atendimento);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var atendimento = await _context.Atendimentos.FindAsync(id);
            if (atendimento != null)
            {
                _context.Atendimentos.Remove(atendimento);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}