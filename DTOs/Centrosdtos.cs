namespace ApiVacunas.DTOs
{
    // ============================================================
    // REQUEST DTOs -> solo admin puede agregar o modificar centros
    //==============================================================

    public class CrearCentroDto
    {

        public string Nombre { get; set; } = string.Empty;

        public string? Direccion { get; set; }

        public decimal? Latitud { get; set; }

        public decimal? Longitud { get; set; }

        public string? Tipo { get; set; }

        public string? Horario { get; set; }

        public string? Telefono { get; set; }

        public bool Activo { get; set; } = true;

    }

    public class ActualizarCentroDto
    {

        public string Nombre { get; set; } = string.Empty;

        public string? Direccion { get; set; }

        public decimal? Latitud { get; set; }

        public decimal? Longitud { get; set; }

        public string? Tipo { get; set; }

        public string? Horario { get; set; }

        public string? Telefono { get; set; }

        public bool Activo { get; set; } = true;

    }

    // ============================================================
    // RESPONSE DTOs
    // ============================================================

    public class CentroDto
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
    }

}