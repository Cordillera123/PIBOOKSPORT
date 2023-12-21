using System;
using System.Collections.Generic;

namespace IntegradorP.Models;

public partial class Deporte
{
    public int DeporteId { get; set; }

    public string? NombreDep { get; set; }

    public virtual ICollection<Instalacion> Instalacions { get; set; } = new List<Instalacion>();
}
