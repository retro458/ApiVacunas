using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Alergia
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Miembroalergia> Miembroalergia { get; set; } = new List<Miembroalergia>();
}
