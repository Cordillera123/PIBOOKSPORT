using System;
using System.Collections.Generic;

namespace IntegradorP.Models;

public partial class Empresa
{
    public int EmpresaId { get; set; }

    public string? NombreEmp { get; set; }

    public virtual ICollection<UsuarioPerfil> UsuarioPerfils { get; set; } = new List<UsuarioPerfil>();
}
