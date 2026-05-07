using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Miembroalergia
{
    public int Id { get; set; }

    public int IdMiembro { get; set; }

    public int IdAlergia { get; set; }

    public virtual Alergia IdAlergiaNavigation { get; set; } = null!;

    public virtual Miembro IdMiembroNavigation { get; set; } = null!;
}
