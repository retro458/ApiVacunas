using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Vacuna
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string? Fabricante { get; set; }

    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; set; }

    public bool Activo { get; set; } = true;

    public virtual ICollection<Campaniavacunacion> Campaniavacunacions { get; set; } = new List<Campaniavacunacion>();

    public virtual ICollection<Esquemavacuna> Esquemavacunas { get; set; } = new List<Esquemavacuna>();

    public virtual ICollection<Historialvacuna> Historialvacunas { get; set; } = new List<Historialvacuna>();

    public virtual ICollection<Vacunaencentro> Vacunaencentros { get; set; } = new List<Vacunaencentro>();
}
