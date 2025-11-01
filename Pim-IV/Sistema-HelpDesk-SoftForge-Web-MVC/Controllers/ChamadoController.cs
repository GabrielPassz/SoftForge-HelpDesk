using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class ChamadoController : Controller
    {
        private readonly PIMContext _context;
        public ChamadoController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.Chamados.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var chamado = await _context.Chamados.FirstOrDefaultAsync(m => m.ChamadoId == id);
            if (chamado == null) return NotFound();
            return View(chamado);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChamadoId,Protocolo,Titulo,Descricao,DataAbertura,DataFechamento,SLAAtingido,UsuarioSolicitanteId,TecnicoResponsavelId")] Chamado chamado)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chamado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chamado);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado == null) return NotFound();
            return View(chamado);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChamadoId,Protocolo,Titulo,Descricao,DataAbertura,DataFechamento,SLAAtingido,UsuarioSolicitanteId,TecnicoResponsavelId")] Chamado chamado)
        {
            if (id != chamado.ChamadoId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chamado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Chamados.Any(e => e.ChamadoId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chamado);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var chamado = await _context.Chamados.FirstOrDefaultAsync(m => m.ChamadoId == id);
            if (chamado == null) return NotFound();
            return View(chamado);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chamado = await _context.Chamados.FindAsync(id);
            if (chamado != null)
            {
                _context.Chamados.Remove(chamado);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}