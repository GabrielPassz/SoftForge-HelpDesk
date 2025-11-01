using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM.Data;
using PIM.Models;

namespace PIM.Controllers
{
    public class LogAcessoController : Controller
    {
        private readonly PIMContext _context;
        public LogAcessoController(PIMContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.LogsAcesso.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var log = await _context.LogsAcesso.FirstOrDefaultAsync(m => m.LogId == id);
            if (log == null) return NotFound();
            return View(log);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LogId,UsuarioId,DataHora,IPAddress,Descricao,Acao,Dispositivo")] LogAcesso log)
        {
            if (ModelState.IsValid)
            {
                _context.Add(log);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(log);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var log = await _context.LogsAcesso.FindAsync(id);
            if (log == null) return NotFound();
            return View(log);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LogId,UsuarioId,DataHora,IPAddress,Descricao,Acao,Dispositivo")] LogAcesso log)
        {
            if (id != log.LogId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(log);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.LogsAcesso.Any(e => e.LogId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(log);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var log = await _context.LogsAcesso.FirstOrDefaultAsync(m => m.LogId == id);
            if (log == null) return NotFound();
            return View(log);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var log = await _context.LogsAcesso.FindAsync(id);
            if (log != null)
            {
                _context.LogsAcesso.Remove(log);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}