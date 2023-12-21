using System;
using System.Collections.Generic;

namespace IntegradorP.Models;

public partial class Perfil
{
    public int PerfilId { get; set; }

    public string? DescripcionPer { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<UsuarioPerfil> UsuarioPerfils { get; set; } = new List<UsuarioPerfil>();
}
