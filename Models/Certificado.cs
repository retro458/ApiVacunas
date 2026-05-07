using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Certificado
{
    public int Id { get; set; }

    public int IdMiembro { get; set; }

    public string? CodigoQr { get; set; }

    public DateTime FechaEmision { get; set; }

    public string? UrlPdf { get; set; }

    public virtual Miembro IdMiembroNavigation { get; set; } = null!;
}
