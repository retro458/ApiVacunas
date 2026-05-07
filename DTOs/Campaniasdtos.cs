namespace ApiVacunas.DTOs
{
    // ============================================================
    // REQUEST DTOs -> solo admin puede agregar o modificar campañas
    // ============================================================

    public class CrearCampaniaDto
    {

        public string Nombre { get; set; } = string.Empty;

        public string? Lugar { get; set; }

        public DateTime Fecha { get; set; }

        public decimal? Latitud { get; set; }

        public decimal? Longitud { get; set; }

        public int? IdVacuna { get; set; }

        public int? IdCentro { get; set; }

        public bool? Activo { get; set; }
    }

    public class ActualizarCampaniaDto
    {

        public string Nombre { get; set; } = string.Empty;

        public string? Lugar { get; set; }

        public DateTime Fecha { get; set; }

        public decimal? Latitud { get; set; }

        public decimal? Longitud { get; set; }

        public int? IdVacuna { get; set; }

        public int? IdCentro { get; set; }

        public bool? Activo { get; set; }
    }

    // ============================================================
    // RESPONSE DTOs
    // ============================================================

    public class CampaniaDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string? Lugar { get; set; }

        public DateTime Fecha { get; set; }

        public decimal? Latitud { get; set; }

        public decimal? Longitud { get; set; }

        public int? IdVacuna { get; set; }

        public int? IdCentro { get; set; }

        public bool? Activo { get; set; }
    }



}