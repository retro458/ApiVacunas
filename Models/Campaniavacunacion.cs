using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Campaniavacunacion
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Lugar { get; set; }

    public DateTime Fecha { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public int? IdVacuna { get; set; }

    public int? IdCentro { get; set; }

    public bool Activo { get; set; } = true;

    public virtual Centrovacunacion? IdCentroNavigation { get; set; }

    public virtual Vacuna? IdVacunaNavigation { get; set; }
    public virtual ICollection<Campaniacentro> Campaniacentros { get; set; } = new List<Campaniacentro>();
}
