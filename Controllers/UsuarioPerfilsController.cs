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
    public class UsuarioPerfilsController : Controller
    {
        private readonly BaseDatosProyectoContext _context;

        public UsuarioPerfilsController(BaseDatosProyectoContext context)
        {
            _context = context;
        }

        // GET: UsuarioPerfils
        public async Task<IActionResult> Index()
        {
            var baseDatosProyectoContext = _context.UsuarioPerfils.Include(u => u.Empresa).Include(u => u.Perfil).Include(u => u.Usuario);
            return View(await baseDatosProyectoContext.ToListAsync());
        }

        // GET: UsuarioPerfils/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.UsuarioPerfils == null)
            {
                return NotFound();
            }

            var usuarioPerfil = await _context.UsuarioPerfils
                .Include(u => u.Empresa)
                .Include(u => u.Perfil)
                .Include(u => u.Usuario)
                .FirstOrDefaultAsync(m => m.Upid == id);
            if (usuarioPerfil == null)
            {
                return NotFound();
            }

            return View(usuarioPerfil);
        }

        // GET: UsuarioPerfils/Create
        public IActionResult Create()
        {
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "EmpresaId", "EmpresaId");
            ViewData["PerfilId"] = new SelectList(_context.Perfils, "PerfilId", "PerfilId");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId");
            return View();
        }

        // POST: UsuarioPerfils/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Upid,UsuarioId,PerfilId,EmpresaId")] UsuarioPerfil usuarioPerfil)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuarioPerfil);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "EmpresaId", "EmpresaId", usuarioPerfil.EmpresaId);
            ViewData["PerfilId"] = new SelectList(_context.Perfils, "PerfilId", "PerfilId", usuarioPerfil.PerfilId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", usuarioPerfil.UsuarioId);
            return View(usuarioPerfil);
        }

        // GET: UsuarioPerfils/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.UsuarioPerfils == null)
            {
                return NotFound();
            }

            var usuarioPerfil = await _context.UsuarioPerfils.FindAsync(id);
            if (usuarioPerfil == null)
            {
                return NotFound();
            }
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "EmpresaId", "EmpresaId", usuarioPerfil.EmpresaId);
            ViewData["PerfilId"] = new SelectList(_context.Perfils, "PerfilId", "PerfilId", usuarioPerfil.PerfilId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", usuarioPerfil.UsuarioId);
            return View(usuarioPerfil);
        }

        // POST: UsuarioPerfils/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Upid,UsuarioId,PerfilId,EmpresaId")] UsuarioPerfil usuarioPerfil)
        {
            if (id != usuarioPerfil.Upid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuarioPerfil);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioPerfilExists(usuarioPerfil.Upid))
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
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "EmpresaId", "EmpresaId", usuarioPerfil.EmpresaId);
            ViewData["PerfilId"] = new SelectList(_context.Perfils, "PerfilId", "PerfilId", usuarioPerfil.PerfilId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", usuarioPerfil.UsuarioId);
            return View(usuarioPerfil);
        }

        // GET: UsuarioPerfils/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.UsuarioPerfils == null)
            {
                return NotFound();
            }

            var usuarioPerfil = await _context.UsuarioPerfils
                .Include(u => u.Empresa)
                .Include(u => u.Perfil)
                .Include(u => u.Usuario)
                .FirstOrDefaultAsync(m => m.Upid == id);
            if (usuarioPerfil == null)
            {
                return NotFound();
            }

            return View(usuarioPerfil);
        }

        // POST: UsuarioPerfils/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.UsuarioPerfils == null)
            {
                return Problem("Entity set 'BaseDatosProyectoContext.UsuarioPerfils'  is null.");
            }
            var usuarioPerfil = await _context.UsuarioPerfils.FindAsync(id);
            if (usuarioPerfil != null)
            {
                _context.UsuarioPerfils.Remove(usuarioPerfil);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioPerfilExists(int id)
        {
          return (_context.UsuarioPerfils?.Any(e => e.Upid == id)).GetValueOrDefault();
        }
    }
}
