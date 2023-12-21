using System;
using System.Collections.Generic;

namespace IntegradorP.Models;

public partial class Reserva
{
    public int ReservaId { get; set; }

    public DateTime? Fecha { get; set; }

    public TimeSpan? HoraInicio { get; set; }

    public TimeSpan? HoraFin { get; set; }

    public int? UsuarioId { get; set; }

    public double? Precio { get; set; }

    public int? InstalacionId { get; set; }

    public virtual Instalacion? Instalacion { get; set; }

    public virtual ICollection<ReservaRecurso> ReservaRecursos { get; set; } = new List<ReservaRecurso>();

    public virtual Usuario? Usuario { get; set; }
}
