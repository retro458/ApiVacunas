using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Centrovacunacion
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Direccion { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public string? Tipo { get; set; }

    public string? Horario { get; set; }

    public string? Telefono { get; set; }

    public bool Activo { get; set; } = true;

    public virtual ICollection<Campaniavacunacion> Campaniavacunacions { get; set; } = new List<Campaniavacunacion>();

    public virtual ICollection<Historialvacuna> Historialvacunas { get; set; } = new List<Historialvacuna>();

    public virtual ICollection<Vacunaencentro> Vacunaencentros { get; set; } = new List<Vacunaencentro>();

    public virtual ICollection<Campaniacentro> Campaniacentros { get; set; } = new List<Campaniacentro>();
}
