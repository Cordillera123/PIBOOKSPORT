using System;
using System.Collections.Generic;

namespace IntegradorP.Models;

public partial class Instalacion
{
    public int InstalacionId { get; set; }

    public string? NombreIns { get; set; }

    public string? DireccionIns { get; set; }

    public string? DescripcioIns { get; set; }

    public int? DeporteId { get; set; }

    public virtual Deporte? Deporte { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
