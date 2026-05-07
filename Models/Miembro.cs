using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Miembro
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public DateTime? FechaNacimiento { get; set; }

    public string? Genero { get; set; }

    public string? Especie { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? FotoUrl { get; set; }

    public bool Activo { get; set; } = true;

    public virtual ICollection<Carnetescaneado> Carnetescaneados { get; set; } = new List<Carnetescaneado>();

    public virtual ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();

    public virtual ICollection<Historialvacuna> Historialvacunas { get; set; } = new List<Historialvacuna>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Imc> Imcs { get; set; } = new List<Imc>();

    public virtual ICollection<Miembroalergia> Miembroalergia { get; set; } = new List<Miembroalergia>();
}
