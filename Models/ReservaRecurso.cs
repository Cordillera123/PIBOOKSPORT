using System;
using System.Collections.Generic;

namespace IntegradorP.Models;

public partial class ReservaRecurso
{
    public int Rrid { get; set; }

    public int? ReservaId { get; set; }

    public int? RecursoId { get; set; }

    public double? ValorTotalRecurso { get; set; }

    public int? Cantidad { get; set; }

    public virtual Recurso? Recurso { get; set; }

    public virtual Reserva? Reserva { get; set; }
}
