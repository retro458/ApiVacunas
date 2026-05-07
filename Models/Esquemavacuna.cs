using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Esquemavacuna
{
    public int Id { get; set; }

    public int IdVacuna { get; set; }

    public int NumeroDosis { get; set; }

    public int? IntervaloDias { get; set; }

    public int? EdadMinimaDias { get; set; }

    public string? Descripcion { get; set; }

    public virtual Vacuna IdVacunaNavigation { get; set; } = null!;
}
