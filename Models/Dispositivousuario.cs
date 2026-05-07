using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Dispositivousuario
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public string TokenPush { get; set; } = null!;

    public string Plataforma { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
