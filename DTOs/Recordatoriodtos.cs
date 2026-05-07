namespace ApiVacunas.DTOs
{
    // ============================================================
    // REQUEST DTOs
    // ============================================================

    public class ActualizarRecordatorioDto
    {
        public string Estado { get; set; } = string.Empty; // pendiente | completado | pospuesto
    }

    public class CrearRecordatorioCampaniaDto
    {
        public int      IdCampania         { get; set; }
        public DateTime FechaRecordatorio  { get; set; }
        public string?  Mensaje            { get; set; }
    }

    // ============================================================
    // RESPONSE DTOs
    // ============================================================

    public class RecordatorioDto
    {
        public int       Id                 { get; set; }
        public int       IdHistorial        { get; set; }
        public DateTime  FechaRecordatorio  { get; set; }
        public string    Tipo               { get; set; } = string.Empty;
        public string?   Mensaje            { get; set; }
        public string    Estado             { get; set; } = string.Empty;
        public string?   Vacuna             { get; set; }
        public string?   Miembro            { get; set; }
        public int?      DiasRestantes      { get; set; }
    }

    public class RecordatorioCampaniaDto
    {
        public int       Id                { get; set; }
        public string    Campania          { get; set; } = string.Empty;
        public string?   Vacuna            { get; set; }
        public string?   Lugar             { get; set; }
        public DateTime  FechaCampania     { get; set; }
        public DateTime  FechaRecordatorio { get; set; }
        public int?      DiasRestantes     { get; set; }
    }
}