namespace ApiVacunas.DTOs
{
    // ============================================================
    // REQUEST DTOs
    // ============================================================

    public class CrearMiembroDto
    {
        public string  Nombre           { get; set; } = string.Empty;
        public string  Tipo             { get; set; } = string.Empty; // persona | mascota
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero           { get; set; }
        public string? Especie          { get; set; }  // solo mascotas
        public string? NumeroDocumento  { get; set; }
        public string? FotoUrl          { get; set; }
    }

    public class ActualizarMiembroDto
    {
        public string  Nombre           { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero           { get; set; }
        public string? Especie          { get; set; }
        public string? NumeroDocumento  { get; set; }
        public string? FotoUrl          { get; set; }
    }

    // ============================================================
    // RESPONSE DTOs
    // ============================================================

    public class MiembroDto
    {
        public int      Id              { get; set; }
        public string   Nombre          { get; set; } = string.Empty;
        public string   Tipo            { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string?  Genero          { get; set; }
        public string?  Especie         { get; set; }
        public string?  NumeroDocumento { get; set; }
        public string?  FotoUrl         { get; set; }
        public int?     Edad            { get; set; }
    }
}