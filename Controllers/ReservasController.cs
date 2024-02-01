using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using IntegradorP.Models;

namespace IntegradorP.Controllers
{
    public class ReservasController : Controller
    {
        private readonly BaseDatosProyectoContext _context;

        public ReservasController(BaseDatosProyectoContext context)
        {
            _context = context;
        }

// GET: Reservas
public async Task<IActionResult> Index()
        {
            var baseDatosProyectoContext = _context.Reservas.Include(r => r.Instalacion).Include(r => r.Usuario);
            return View(await baseDatosProyectoContext.ToListAsync());
        }

// GET: Reservas/Details/5
public async Task<IActionResult> Details(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var reserva = await _context.Reservas
        .Include(r => r.ReservaRecursos) // Incluye los ReservaRecurso relacionados
            .ThenInclude(rr => rr.Recurso) // Incluye el Recurso relacionado con cada ReservaRecurso
        .FirstOrDefaultAsync(m => m.ReservaId == id);

    if (reserva == null)
    {
        return NotFound();
    }

    return View(reserva);
}
        // GET: Reservas/Create
        public IActionResult Create()
        {
            ViewData["InstalacionId"] = new SelectList(_context.Instalacions, "InstalacionId", "InstalacionId");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId");
            return View(new Reserva());
        }


// POST: Reservas/Create
[HttpPost]
[ValidateAntiForgeryToken]

public async Task<IActionResult> Create([Bind("ReservaId,Fecha,HoraInicio,HoraFin,UsuarioId,InstalacionId")] Reserva reserva)
{
    // Verifica si la fecha y hora de inicio de la reserva son anteriores a la fecha y hora actual
    if ((reserva.Fecha ?? DateTime.MinValue).Date < DateTime.Now.Date || ((reserva.Fecha ?? DateTime.MinValue).Date == DateTime.Now.Date && (reserva.HoraInicio ?? TimeSpan.Zero) <= DateTime.Now.TimeOfDay))
    {
        ModelState.AddModelError("Fecha", "No puedes reservar en fechas pasadas.");
    }

    // Verifica si la hora de fin de la reserva es anterior a la hora de inicio
    if (reserva.HoraFin <= reserva.HoraInicio)
    {
        ModelState.AddModelError("HoraFin", "La hora de fin debe ser posterior a la hora de inicio.");
    }

    // Verifica si ya existe una reserva para la misma instalación en el mismo intervalo de tiempo
    var overlappingReservations = _context.Reservas
        .Where(r => r.InstalacionId == reserva.InstalacionId && r.Fecha == reserva.Fecha && 
                    ((r.HoraInicio <= reserva.HoraInicio && r.HoraFin > reserva.HoraInicio) || 
                     (r.HoraInicio < reserva.HoraFin && r.HoraFin >= reserva.HoraFin) ||
                     (r.HoraInicio >= reserva.HoraInicio && r.HoraFin <= reserva.HoraFin)))
        .ToList();

    if (overlappingReservations.Any())
    {
        ModelState.AddModelError(string.Empty, "La instalación ya está reservada en el mismo intervalo de tiempo.");
    }

    if (ModelState.IsValid)
    {
        // Precio por hora fijo
        var pricePerHour = 5m;

        // Crear los parámetros para el procedimiento almacenado
        var startTimeParam = new SqlParameter("@StartTime", reserva.HoraInicio);
        var endTimeParam = new SqlParameter("@EndTime", reserva.HoraFin);
        var pricePerHourParam = new SqlParameter("@PricePerHour", pricePerHour);

        // Ejecutar el procedimiento almacenado y obtener el precio total
        var totalPriceParam = new SqlParameter
        {
            ParameterName = "@TotalPrice",
            SqlDbType = SqlDbType.Decimal,
            Direction = ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync("EXEC CalculateReservationPrice @StartTime, @EndTime, @PricePerHour, @TotalPrice OUT", 
            startTimeParam, endTimeParam, pricePerHourParam, totalPriceParam);

        // Asigna el precio total a la reserva
        reserva.Precio = Convert.ToDouble(totalPriceParam.Value);

        _context.Add(reserva);
        await _context.SaveChangesAsync();

        // Redirige a la vista AddResource para la reserva recién creada
        return RedirectToAction(nameof(AddResource), new { reservaId = reserva.ReservaId });
    }
    ViewData["InstalacionId"] = new SelectList(_context.Instalacions, "InstalacionId", "InstalacionId", reserva.InstalacionId);
    ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", reserva.UsuarioId);
    return View(reserva);
}
        public IActionResult AddResource(int reservaId)
        {
            // Obtén la reserva
            var reserva = _context.Reservas.Find(reservaId);

            // Obtén todos los recursos disponibles
            var recursos = _context.Recursos.ToList();

            // Pasa la reserva y los recursos a la vista
            ViewData["Reserva"] = reserva;
            ViewData["Recursos"] = new SelectList(recursos, "RecursoId", "NombreRec");

            return View();
        }

        [HttpPost]
public IActionResult AddResourceToReservation(int reservaId, List<int> recursoId, List<int> cantidad)
{
    var reserva = _context.Reservas.Find(reservaId);

    if (recursoId != null && cantidad != null)
    {
        for (int i = 0; i < recursoId.Count; i++)
        {
            var recurso = _context.Recursos.Find(recursoId[i]);

            var reservaRecurso = new ReservaRecurso
            {
                ReservaId = reservaId,
                RecursoId = recursoId[i],
                Cantidad = cantidad[i],
                ValorTotalRecurso = recurso.ValorUnitario * cantidad[i]
            };

            _context.ReservaRecursos.Add(reservaRecurso);
        }

        double valorTotalRecursos = reserva.ReservaRecursos.Sum(rr => rr.ValorTotalRecurso ?? 0.0);
        reserva.ValorTotalReserva = (reserva.Precio ?? 0.0) + valorTotalRecursos;

        _context.SaveChanges();
    }

    return RedirectToAction("Details", new { id = reservaId });
}

// GET: Reservas/Edit/5
public async Task<IActionResult> Edit(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var reserva = await _context.Reservas.FindAsync(id);
    if (reserva == null)
    {
        return NotFound();
    }
    ViewData["InstalacionId"] = new SelectList(_context.Instalacions, "InstalacionId", "InstalacionId", reserva.InstalacionId);
    ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", reserva.UsuarioId);
    return View(reserva);
}

       


// POST: Reservas/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("ReservaId,Fecha,HoraInicio,HoraFin,UsuarioId,InstalacionId")] Reserva reserva)
{
    if (id != reserva.ReservaId)
    {
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        try
        {
            _context.Update(reserva);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReservaExists(reserva.ReservaId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Create));
    }
    ViewData["InstalacionId"] = new SelectList(_context.Instalacions, "InstalacionId", "InstalacionId", reserva.InstalacionId);
    ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", reserva.UsuarioId);
    return View(reserva);
}

// GET: Reservas/Delete/5
public async Task<IActionResult> Delete(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var reserva = await _context.Reservas.FindAsync(id);
    if (reserva == null)
    {
        return NotFound();
    }

    return View(reserva);
}

// POST: Reservas/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var reserva = await _context.Reservas.FindAsync(id);
    _context.Reservas.Remove(reserva);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}

 private bool ReservaExists(int id)
        {
          return (_context.Reservas?.Any(e => e.ReservaId == id)).GetValueOrDefault();
        }
}

}