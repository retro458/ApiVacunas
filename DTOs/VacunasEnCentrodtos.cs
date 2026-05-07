namespace ApiVacunas.DTOs
{
    // ============================================================
    // REQUEST DTOs
    // ============================================================

    public class AsignarVacunaCentroDto
    {
        public int      IdCentro    { get; set; }
        public int      IdVacuna    { get; set; }
        public bool     Disponible  { get; set; } = true;
        public decimal? Precio      { get; set; }
    }

    public class ActualizarVacunaCentroDto
    {
        public bool     Disponible  { get; set; }
        public decimal? Precio      { get; set; }
    }

    // ============================================================
    // RESPONSE DTOs
    // ============================================================

    public class VacunaCentroDto
    {
        public int      Id          { get; set; }
        public int      IdCentro    { get; set; }
        public string   Centro      { get; set; } = string.Empty;
        public int      IdVacuna    { get; set; }
        public string   Vacuna      { get; set; } = string.Empty;
        public string?  Fabricante  { get; set; }
        public string?  TipoVacuna  { get; set; }
        public bool     Disponible  { get; set; }
        public decimal? Precio      { get; set; }
    }
}