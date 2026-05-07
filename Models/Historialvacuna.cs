using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Historialvacuna
{
    public int Id { get; set; }

    public int IdMiembro { get; set; }

    public int IdVacuna { get; set; }

    public int? IdCentro { get; set; }

    public DateTime FechaAplicacion { get; set; }

    public DateTime? ProximaDosis { get; set; }

    public int DosisNumero { get; set; }

    public string? Lote { get; set; }

    public string? NombreMedico { get; set; }

    public string? Observaciones { get; set; }

    public virtual Centrovacunacion? IdCentroNavigation { get; set; }

    public virtual Miembro IdMiembroNavigation { get; set; } = null!;

    public virtual Vacuna IdVacunaNavigation { get; set; } = null!;

    public virtual ICollection<Recordatorio> Recordatorios { get; set; } = new List<Recordatorio>();
}
