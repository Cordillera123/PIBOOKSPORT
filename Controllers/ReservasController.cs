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


public async Task<IActionResult> Index()
{
    var reservas = await _context.Reservas
        .Include(r => r.Usuario)
        .Include(r => r.Instalacion)
        .ToListAsync();

    if (reservas != null)
    {
        return View(reservas);
    }
    else
    {
        return Problem("Entity set 'BaseDatosProyectoContext.Reservas' is null.");
    }
}

// GET: Reservas/Details/5
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
        .Include(r => r.Usuario) // Incluye el Usuario relacionado
        .Include(r => r.Instalacion) // Incluye la Instalacion relacionada
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
            ViewData["InstalacionId"] = new SelectList(_context.Instalacions, "InstalacionId", "NombreIns");
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
    else
    {
      // Carga los datos para los campos UsuarioId e InstalacionId
    ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", reserva.UsuarioId);
    ViewData["InstalacionId"] = new SelectList(_context.Instalacions, "InstalacionId", "NombreIns", reserva.InstalacionId);
    }
    return View(reserva);
}
public async Task<IActionResult> RemoveResource(int reservaId, int recursoId)
{
    var reservaRecurso = await _context.ReservaRecursos
        .FirstOrDefaultAsync(rr => rr.ReservaId == reservaId && rr.RecursoId == recursoId);

    if (reservaRecurso == null)
    {
        return NotFound();
    }

    _context.ReservaRecursos.Remove(reservaRecurso);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Edit), new { id = reservaId });
}
public IActionResult AddResourceView(int reservaId)
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
// GET: Reservas/AddResource/5
public async Task<IActionResult> AddResource(int? reservaId)
{
    if (reservaId == null)
    {
        return NotFound();
    }

    var reserva = await _context.Reservas.FindAsync(reservaId);

    if (reserva == null)
    {
        return NotFound();
    }

    // Obtén todos los recursos disponibles
    var recursos = await _context.Recursos.ToListAsync();

    // Pasa la reserva y los recursos a la vista
    ViewData["Reserva"] = reserva;
    ViewData["Recursos"] = new SelectList(recursos, "RecursoId", "NombreRec");
    ViewData["IsEditing"] = true; // Indica que la reserva está en proceso de edición

    return View();
}
// POST: Reservas/AddResource
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddResourceToReservation(int ReservaId, List<int> RecursoId, List<int> Cantidad)
{
    var reserva = await _context.Reservas
        .Include(r => r.ReservaRecursos)
        .FirstOrDefaultAsync(r => r.ReservaId == ReservaId);

    if (reserva == null)
    {
        return NotFound();
    }

    for (int i = 0; i < RecursoId.Count; i++)
    {
        var recurso = await _context.Recursos.FindAsync(RecursoId[i]);

        if (recurso == null)
        {
            return NotFound();
        }

        var existingReservaRecurso = reserva.ReservaRecursos
            .FirstOrDefault(rr => rr.RecursoId == RecursoId[i]);

        if (existingReservaRecurso != null)
        {
            // Si el recurso ya existe en la reserva, actualiza la cantidad
            existingReservaRecurso.Cantidad += Cantidad[i];
            existingReservaRecurso.ValorTotalRecurso = recurso.ValorUnitario * existingReservaRecurso.Cantidad;
        }
        else
        {
            // Si el recurso no existe en la reserva, añádelo
            var reservaRecurso = new ReservaRecurso
            {
                ReservaId = ReservaId,
                RecursoId = RecursoId[i],
                Cantidad = Cantidad[i],
                ValorTotalRecurso = recurso.ValorUnitario * Cantidad[i]
            };
            _context.ReservaRecursos.Add(reservaRecurso);
        }
    }

    await _context.SaveChangesAsync();

    // Redirige a la vista Details para la reserva actual
    return RedirectToAction(nameof(Details), new { id = ReservaId });
}

        // GET: Reservas/Edit/5
public async Task<IActionResult> Edit(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var reserva = await _context.Reservas
        .Include(r => r.ReservaRecursos)
            .ThenInclude(rr => rr.Recurso)
        .FirstOrDefaultAsync(m => m.ReservaId == id);

    if (reserva == null)
    {
        return NotFound();
    }

    ViewData["InstalacionId"] = new SelectList(await _context.Instalacions.ToListAsync(), "InstalacionId", "NombreIns", reserva.InstalacionId);
    ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", reserva.UsuarioId);
    ViewData["Recursos"] = new SelectList(_context.Recursos, "RecursoId", "NombreRec");
    ViewData["ReservaRecursos"] = reserva.ReservaRecursos; // Añade los recursos de la reserva a ViewData

// Añade la variable para ocultar el botón "Agregar recurso"
    ViewData["HideAddResourceButton"] = true;
    return View(reserva);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Reserva reserva, int[] RecursoId, int[] Cantidad)
{
    if (id != reserva.ReservaId)
    {
        return NotFound();
    }
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
                     (r.HoraInicio >= reserva.HoraInicio && r.HoraFin <= reserva.HoraFin)) &&
                     r.ReservaId != id) // Excluye la reserva actual de la verificación
        .ToList();

    if (overlappingReservations.Any())
    {
        ModelState.AddModelError(string.Empty, "La instalación ya está reservada en el mismo intervalo de tiempo.");
    }


    if (ModelState.IsValid)
    {
        try
        {
            // Obtén la reserva existente de la base de datos
            var existingReserva = await _context.Reservas
                .Include(r => r.ReservaRecursos)
                .FirstOrDefaultAsync(r => r.ReservaId == id);

            if (existingReserva == null)
            {
                return NotFound();
            }

            // Actualiza los campos de la reserva
            existingReserva.Fecha = reserva.Fecha;
            existingReserva.HoraInicio = reserva.HoraInicio;
            existingReserva.HoraFin = reserva.HoraFin;
            existingReserva.UsuarioId = reserva.UsuarioId;
            existingReserva.InstalacionId = reserva.InstalacionId;

            // Actualiza los recursos existentes con los nuevos valores
            for (int i = 0; i < RecursoId.Length; i++)
            {
                var existingReservaRecurso = existingReserva.ReservaRecursos
                    .FirstOrDefault(rr => rr.RecursoId == RecursoId[i]);

                if (existingReservaRecurso != null)
                {
                    existingReservaRecurso.Cantidad += Cantidad[i];
                }
                else
                {
                    // Si el recurso no existe en la reserva, añádelo
                    var reservaRecurso = new ReservaRecurso
                    {
                        ReservaId = id,
                        RecursoId = RecursoId[i],
                        Cantidad = Cantidad[i]
                    };
                    _context.ReservaRecursos.Add(reservaRecurso);
                }
            }

             _context.Update(existingReserva);
            await _context.SaveChangesAsync();

            // Precio por hora fijo
            var pricePerHour = 5m;

            // Crear los parámetros para el procedimiento almacenado
            var startTimeParam = new SqlParameter("@StartTime", existingReserva.HoraInicio);
            var endTimeParam = new SqlParameter("@EndTime", existingReserva.HoraFin);
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
            existingReserva.Precio = Convert.ToDouble(totalPriceParam.Value);

            _context.Update(existingReserva);
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
       return RedirectToAction(nameof(AddResource), new { reservaId = reserva.ReservaId });
    }
    else
    {
         // Carga los datos para los campos UsuarioId e InstalacionId
    ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", reserva.UsuarioId);
    ViewData["InstalacionId"] = new SelectList(_context.Instalacions, "InstalacionId", "NombreIns", reserva.InstalacionId);
    }
    return View(reserva);
}


   // GET: Reservas1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Reservas == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Instalacion)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(m => m.ReservaId == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

      // POST: Reservas1/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    if (_context.Reservas == null)
    {
        return Problem("Entity set 'BaseDatosProyectoContext.Reservas' is null.");
    }

    try
    {
        // Busca la reserva
        var reserva = await _context.Reservas
            .Include(r => r.ReservaRecursos) // Incluye los ReservaRecurso relacionados
            .FirstOrDefaultAsync(m => m.ReservaId == id);

        if (reserva == null)
        {
            return NotFound();
        }

        // Elimina los registros asociados en ReservaRecurso
        _context.ReservaRecursos.RemoveRange(reserva.ReservaRecursos);

        // Luego elimina la reserva
        _context.Reservas.Remove(reserva);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        // Manejar cualquier excepción que pueda ocurrir durante la eliminación
        // Puedes registrar el error o mostrar un mensaje de error al usuario
        return Problem($"Error al eliminar la reserva: {ex.Message}");
    }
}
        private bool ReservaExists(int id)
        {
          return (_context.Reservas?.Any(e => e.ReservaId == id)).GetValueOrDefault();
        }
}

}