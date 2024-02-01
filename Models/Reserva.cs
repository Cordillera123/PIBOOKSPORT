using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntegradorP.Models;

public partial class Reserva
{
    public int ReservaId { get; set; }

    [Required(ErrorMessage = "La fecha es requerida.")]
    public DateTime? Fecha { get; set; }

    [Required(ErrorMessage = "La hora de inicio es requerida.")]
    public TimeSpan? HoraInicio { get; set; }

    [Required(ErrorMessage = "La hora de fin es requerida.")]
    public TimeSpan? HoraFin { get; set; }

    public int? UsuarioId { get; set; }

    public double? Precio { get; set; }

    public int? InstalacionId { get; set; }
    public double ValorTotalReserva { get; set; }

    public virtual Instalacion? Instalacion { get; set; }

    public virtual ICollection<ReservaRecurso> ReservaRecursos { get; set; } = new List<ReservaRecurso>();

    public virtual Usuario? Usuario { get; set; }
}
