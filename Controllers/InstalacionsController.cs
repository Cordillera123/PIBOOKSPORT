using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IntegradorP.Models;

namespace IntegradorP.Controllers
{
    public class InstalacionsController : Controller
    {
        private readonly BaseDatosProyectoContext _context;

        public InstalacionsController(BaseDatosProyectoContext context)
        {
            _context = context;
        }

        // GET: Instalacions
        public async Task<IActionResult> Index()
        {
            var baseDatosProyectoContext = _context.Instalacions.Include(i => i.Deporte);
            return View(await baseDatosProyectoContext.ToListAsync());
        }

        // GET: Instalacions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Instalacions == null)
            {
                return NotFound();
            }

            var instalacion = await _context.Instalacions
                .Include(i => i.Deporte)
                .FirstOrDefaultAsync(m => m.InstalacionId == id);
            if (instalacion == null)
            {
                return NotFound();
            }

            return View(instalacion);
        }

        // GET: Instalacions/Create
        public IActionResult Create()
        {
            ViewData["DeporteId"] = new SelectList(_context.Deportes, "DeporteId", "DeporteId");
            return View();
        }

        // POST: Instalacions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InstalacionId,NombreIns,DireccionIns,DescripcioIns,DeporteId")] Instalacion instalacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(instalacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeporteId"] = new SelectList(_context.Deportes, "DeporteId", "DeporteId", instalacion.DeporteId);
            return View(instalacion);
        }

        // GET: Instalacions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Instalacions == null)
            {
                return NotFound();
            }

            var instalacion = await _context.Instalacions.FindAsync(id);
            if (instalacion == null)
            {
                return NotFound();
            }
            ViewData["DeporteId"] = new SelectList(_context.Deportes, "DeporteId", "DeporteId", instalacion.DeporteId);
            return View(instalacion);
        }

        // POST: Instalacions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InstalacionId,NombreIns,DireccionIns,DescripcioIns,DeporteId")] Instalacion instalacion)
        {
            if (id != instalacion.InstalacionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instalacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstalacionExists(instalacion.InstalacionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeporteId"] = new SelectList(_context.Deportes, "DeporteId", "DeporteId", instalacion.DeporteId);
            return View(instalacion);
        }

        // GET: Instalacions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Instalacions == null)
            {
                return NotFound();
            }

            var instalacion = await _context.Instalacions
                .Include(i => i.Deporte)
                .FirstOrDefaultAsync(m => m.InstalacionId == id);
            if (instalacion == null)
            {
                return NotFound();
            }

            return View(instalacion);
        }

        // POST: Instalacions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Instalacions == null)
            {
                return Problem("Entity set 'BaseDatosProyectoContext.Instalacions'  is null.");
            }
            var instalacion = await _context.Instalacions.FindAsync(id);
            if (instalacion != null)
            {
                _context.Instalacions.Remove(instalacion);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InstalacionExists(int id)
        {
          return (_context.Instalacions?.Any(e => e.InstalacionId == id)).GetValueOrDefault();
        }
    }
}
