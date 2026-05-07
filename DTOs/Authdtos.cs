namespace ApiVacunas.DTOs
{
    // ============================================================
    // REQUEST DTOs - Lo que recibe la API desde Android
    // ============================================================

    public class LoginDto
    {
        public string Correo    { get; set; } = string.Empty;
        public string Password  { get; set; } = string.Empty;
    }

    public class RegistroDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Rol { get; set; }
    }
    
    public class GoogleAuthDto
    {
    public string IdToken { get; set; } = string.Empty; // token que manda Android
    }

    // ============================================================
    // RESPONSE DTOs - Lo que devuelve la API a Android
    // ============================================================

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
    }

    public class RespuestaDto
    {
        public bool   Exito    { get; set; }
        public string Mensaje  { get; set; } = string.Empty;
        public object? Data    { get; set; }
    }
}