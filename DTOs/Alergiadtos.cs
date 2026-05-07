namespace ApiVacunas.DTOs
{
    // ============================================================
    // REQUEST DTOs
    // ============================================================

    public class CrearAlergiaDto
    {
        public string Nombre { get; set; } = string.Empty;
    }

    public class AsignarAlergiaDto
    {
        public int IdAlergia { get; set; }
    }

    // ============================================================
    // RESPONSE DTOs
    // ============================================================

    public class AlergiaDto
    {
        public int    Id     { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class MiembroAlergiaDto
    {
        public int    IdMiembroAlergia { get; set; }
        public int    IdAlergia        { get; set; }
        public string Nombre           { get; set; } = string.Empty;
    }
}