using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Vacunaencentro
{
    public int Id { get; set; }

    public int IdCentro { get; set; }

    public int IdVacuna { get; set; }

    public bool Disponible { get; set; } = true;

    public decimal? Precio { get; set; }

    public virtual Centrovacunacion IdCentroNavigation { get; set; } = null!;

    public virtual Vacuna IdVacunaNavigation { get; set; } = null!;
}
