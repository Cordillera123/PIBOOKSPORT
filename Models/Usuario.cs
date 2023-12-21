using System;
using System.Collections.Generic;

namespace IntegradorP.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string? NombreUsu { get; set; }

    public string? CedulaUsu { get; set; }

    public string? EmailUsu { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<UsuarioPerfil> UsuarioPerfils { get; set; } = new List<UsuarioPerfil>();
}
