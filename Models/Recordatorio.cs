using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Recordatorio
{
    public int Id { get; set; }

    public int IdHistorial { get; set; }

    public DateTime FechaRecordatorio { get; set; }

    public string Tipo { get; set; } = null!;

    public string? Mensaje { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Historialvacuna IdHistorialNavigation { get; set; } = null!;
}
