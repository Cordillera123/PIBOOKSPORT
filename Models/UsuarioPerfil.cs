using System;
using System.Collections.Generic;

namespace IntegradorP.Models;

public partial class UsuarioPerfil
{
    public int Upid { get; set; }

    public int? UsuarioId { get; set; }

    public int? PerfilId { get; set; }

    public int? EmpresaId { get; set; }

    public virtual Empresa? Empresa { get; set; }

    public virtual Perfil? Perfil { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
