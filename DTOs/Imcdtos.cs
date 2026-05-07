namespace ApiVacunas.DTOs
{
    //============================================================
    // REQUEST DTOs
    // ============================================================
 
    public class RegistrarImcDto
    {
        public int     IdMiembro { get; set; }
        public decimal Peso      { get; set; }  // en kg
        public decimal Altura    { get; set; }  // en metros (ej: 1.75)
        public DateTime Fecha    { get; set; }
    }
 
    public class RegistrarCarnetDto
    {
        public int     IdMiembro     { get; set; }
        public string? NombreClinica { get; set; }
        public DateTime? Fecha       { get; set; }
        public string? Imagen        { get; set; }  // URL del storage
    }
 
    // ============================================================
    // RESPONSE DTOs
    // ============================================================
 
    public class ImcDto
    {
        public int      Id             { get; set; }
        public int      IdMiembro      { get; set; }
        public decimal  Peso           { get; set; }
        public decimal  Altura         { get; set; }
        public decimal? Resultado      { get; set; }
        public string   Clasificacion  { get; set; } = string.Empty;
        public DateTime Fecha          { get; set; }
    }
 
    public class CarnetEscaneadoDto
    {
        public int       Id            { get; set; }
        public int       IdMiembro     { get; set; }
        public string?   NombreClinica { get; set; }
        public DateTime? Fecha         { get; set; }
        public string?   Imagen        { get; set; }
    }
}



