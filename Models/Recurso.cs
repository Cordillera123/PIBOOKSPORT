using System;
using System.Collections.Generic;

namespace IntegradorP.Models;

public partial class Recurso
{
    public int RecursoId { get; set; }

    public string? NombreRec { get; set; }

    public string? DescripcionRec { get; set; }

    public double? ValorUnitario { get; set; }

    public virtual ICollection<ReservaRecurso> ReservaRecursos { get; set; } = new List<ReservaRecurso>();
}
