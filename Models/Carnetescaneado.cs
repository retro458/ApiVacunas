using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Carnetescaneado
{
    public int Id { get; set; }

    public int IdMiembro { get; set; }

    public string? NombreClinica { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Imagen { get; set; }

    public virtual Miembro IdMiembroNavigation { get; set; } = null!;
}
