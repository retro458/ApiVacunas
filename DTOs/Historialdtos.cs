namespace ApiVacunas.DTOs
{
    // ============================================================
    // REQUEST DTOs
    // ============================================================

    public class RegistrarVacunacionDto
    {
        public int      IdMiembro       { get; set; }
        public int      IdVacuna        { get; set; }
        public int?     IdCentro        { get; set; }
        public DateTime FechaAplicacion { get; set; }
        public int      DosisNumero     { get; set; }
        public string?  Lote            { get; set; }
        public string?  NombreMedico    { get; set; }
        public string?  Observaciones   { get; set; }
    }

    // ============================================================
    // RESPONSE DTOs
    // ============================================================

    public class HistorialDto
    {
        public int       Id              { get; set; }
        public DateTime  FechaAplicacion { get; set; }
        public DateTime? ProximaDosis    { get; set; }
        public int       DosisNumero     { get; set; }
        public int       TotalDosis       { get; set; }
        public string?   Lote            { get; set; }
        public string?   NombreMedico    { get; set; }
        public string?   Observaciones   { get; set; }
        public string    Vacuna          { get; set; } = string.Empty;
        public string?   Fabricante      { get; set; }
        public string?   TipoVacuna      { get; set; }
        public string?   Centro          { get; set; }
        public string?   CentroDireccion { get; set; }
        public string?   RecordatorioEstado { get; set; }
        public DateTime? FechaRecordatorio  { get; set; }
    }

    public class ProximaDosisDto
    {
        public string    Miembro        { get; set; } = string.Empty;
        public string    TipoMiembro    { get; set; } = string.Empty;
        public string    Vacuna         { get; set; } = string.Empty;
        public int       DosisAplicada  { get; set; }
        public DateTime  ProximaDosis   { get; set; }
        public int?      DiasRestantes  { get; set; }
        public string?   Recordatorio   { get; set; }
    }
}