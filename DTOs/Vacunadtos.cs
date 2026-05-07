namespace ApiVacunas.DTOs
{
    // ============================================================
    // REQUEST DTOs
    // ============================================================

    public class CrearVacunaDto
    {
        public string  Nombre      { get; set; } = string.Empty;
        public string  Tipo        { get; set; } = string.Empty; // humano | animal | ambos
        public string? Fabricante  { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenUrl   { get; set; }
    }

    public class ActualizarVacunaDto
    {
        public string  Nombre      { get; set; } = string.Empty;
        public string  Tipo        { get; set; } = string.Empty;
        public string? Fabricante  { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenUrl   { get; set; }
    }

    public class CrearEsquemaDto
    {
        public int     IdVacuna      { get; set; }
        public int     NumeroDosis   { get; set; }
        public int?    IntervaloDias { get; set; }
        public int?    EdadMinimaDias { get; set; }
        public string? Descripcion   { get; set; }
    }

    // ============================================================
    // RESPONSE DTOs
    // ============================================================

    public class VacunaDto
    {
        public int     Id          { get; set; }
        public string  Nombre      { get; set; } = string.Empty;
        public string  Tipo        { get; set; } = string.Empty;
        public string? Fabricante  { get; set; }
        public string? Descripcion { get; set; }
        public string? ImagenUrl   { get; set; }
        public bool    Activo      { get; set; }
    }

    public class EsquemaVacunaDto
    {
        public int     Id             { get; set; }
        public int     IdVacuna       { get; set; }
        public int     NumeroDosis    { get; set; }
        public int?    IntervaloDias  { get; set; }
        public int?    EdadMinimaDias { get; set; }
        public string? Descripcion    { get; set; }
    }

    public class VacunaConEsquemaDto
    {
        public VacunaDto          Vacuna  { get; set; } = new();
        public List<EsquemaVacunaDto> Esquemas { get; set; } = new();
    }
}