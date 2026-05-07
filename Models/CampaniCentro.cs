using System;
using System.Collections.Generic;

namespace ApiVacunas.Models;

public partial class Campaniacentro
{
    public int Id { get; set; }

    public int CampaniaId { get; set; }

    public int CentroId { get; set; }

    public virtual Campaniavacunacion IdCampaniaNavigation { get; set; } = null!;

    public virtual Centrovacunacion IdCentroNavigation { get; set; } = null!;
}