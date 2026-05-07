namespace ApiVacunas.DTOs
{
    // ============================================================
    // CERTIFICADO
    // ============================================================
    public class CertificadoDto
    {
        public int      Id           { get; set; }
        public int      IdMiembro    { get; set; }
        public string?  CodigoQr     { get; set; }
        public DateTime FechaEmision { get; set; }
        public string?  UrlPdf       { get; set; }
        public string?  NombreMiembro { get; set; }
    }

    public class CrearCertificadoDto
    {
        public int     IdMiembro { get; set; }
        public string? CodigoQr  { get; set; }
        public string? UrlPdf    { get; set; }
    }

    // ============================================================
    // DISPOSITIVO USUARIO
    // ============================================================
    public class RegistrarDispositivoDto
    {
        public string TokenPush  { get; set; } = string.Empty;
        public string Plataforma { get; set; } = string.Empty; // android | ios
    }

    public class DispositivoDto
    {
        public int    Id          { get; set; }
        public string TokenPush   { get; set; } = string.Empty;
        public string Plataforma  { get; set; } = string.Empty;
        public bool   Activo      { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    // ============================================================
    // DASHBOARD ADMIN
    // ============================================================
    public class DashboardAdminDto
    {
        public int TotalUsuarios            { get; set; }
        public int TotalMiembros            { get; set; }
        public int TotalVacunaciones        { get; set; }
        public int RecordatoriosPendientes  { get; set; }
        public int CampaniasActivas         { get; set; }
        public int CentrosActivos           { get; set; }
    }

    public class VacunacionMesDto
    {
        public string MesNumero  { get; set; } = string.Empty;
        public string MesNombre  { get; set; } = string.Empty;
        public string Vacuna     { get; set; } = string.Empty;
        public int    Total      { get; set; }
    }

    public class CoberturVacunaDto
    {
        public string Vacuna            { get; set; } = string.Empty;
        public string TipoVacuna        { get; set; } = string.Empty;
        public string? Fabricante       { get; set; }
        public int    TotalAplicaciones { get; set; }
        public int    MiembrosVacunados { get; set; }
    }

    // ============================================================
    // DASHBOARD USUARIO — resumen personal
    // ============================================================
    public class DashboardUsuarioDto
    {
        public int TotalMiembros           { get; set; }
        public int TotalVacunaciones       { get; set; }
        public int RecordatoriosPendientes { get; set; }
        public int ProximasDosis30Dias     { get; set; }
        public int CampaniasProximas       { get; set; }
    }

    // ============================================================
    // PERFIL COMPLETO MIEMBRO
    // ============================================================
    public class PerfilMiembroDto
    {
        public MiembroDto                Miembro   { get; set; } = new();
        public List<MiembroAlergiaDto>   Alergias  { get; set; } = new();
        public ImcDto?                   UltimoImc { get; set; }
        public List<HistorialDto>        Historial { get; set; } = new();
        public List<RecordatorioDto>     Recordatorios { get; set; } = new();
    }
}