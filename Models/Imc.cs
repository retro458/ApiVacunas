using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Imc
{
    public int Id { get; set; }

    public int IdMiembro { get; set; }

    public decimal Peso { get; set; }

    public decimal Altura { get; set; }

    public DateTime Fecha { get; set; }

    public decimal? Resultado { get; set; }

    public virtual Miembro IdMiembroNavigation { get; set; } = null!;
}
