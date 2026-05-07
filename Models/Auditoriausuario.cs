using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Auditoriausuario
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public string Campo { get; set; } = null!;

    public string? ValorAnterior { get; set; }

    public string? ValorNuevo { get; set; }

    public DateTime FechaCambio { get; set; }

    public string UsuarioBd { get; set; } = null!;
}
