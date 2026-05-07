using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Telefono { get; set; }

    public string Rol { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<Dispositivousuario> Dispositivousuarios { get; set; } = new List<Dispositivousuario>();

    public virtual ICollection<Miembro> Miembros { get; set; } = new List<Miembro>();
}
