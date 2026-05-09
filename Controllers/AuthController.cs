using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiVacunas.Data;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Types;
using ApiVacunas.DTOs;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;

namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ============================================================
        // POST api/auth/registro
        // ============================================================
        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Correo) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Nombre, correo y password son obligatorios."
                });
            }

            // BCrypt hashea el password antes de mandarlo a Oracle
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_registrar_usuario";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_nombre", OracleDbType.Varchar2).Value = dto.Nombre;
                cmd.Parameters.Add("p_correo", OracleDbType.Varchar2).Value = dto.Correo;
                cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = passwordHash;
                cmd.Parameters.Add("p_telefono", OracleDbType.Varchar2).Value =
                    dto.Telefono ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_rol", OracleDbType.Varchar2).Value =
                    dto.Rol ?? "usuario";

                var pIdOut = new OracleParameter("p_id_out", OracleDbType.Int32)
                { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pIdOut);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var idOut = Convert.ToInt32(pIdOut.Value.ToString());
                var mensaje = pMensaje.Value.ToString()!;

                if (idOut == -1)
                {
                    return Conflict(new RespuestaDto { Exito = false, Mensaje = mensaje });
                }

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = mensaje,
                    Data = new { IdUsuario = idOut }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        // ============================================================
        // POST api/auth/login
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Correo) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Correo y password son obligatorios."
                });
            }

            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_login";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_correo", OracleDbType.Varchar2).Value = dto.Correo;

                var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCursor);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var mensaje = pMensaje.Value.ToString()!;

                if (mensaje != "OK")
                {
                    return Unauthorized(new RespuestaDto { Exito = false, Mensaje = mensaje });
                }

                // Leer datos del usuario desde el cursor
                using var reader = ((OracleRefCursor)pCursor.Value).GetDataReader();

                if (!await reader.ReadAsync())
                {
                    return Unauthorized(new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Usuario no encontrado."
                    });
                }

                var idUsuario = Convert.ToInt32(reader["ID"]);
                var nombre = reader["NOMBRE"].ToString()!;
                var correo = reader["CORREO"].ToString()!;
                var passwordHash = reader["PASSWORD"].ToString()!;
                var rol = reader["ROL"].ToString()!;

                // BCrypt compara el password ingresado con el hash guardado
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, passwordHash))
                {
                    return Unauthorized(new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Credenciales incorrectas."
                    });
                }

                // Generar JWT
                var token = GenerarToken(idUsuario, nombre, correo, rol);

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Login exitoso.",
                    Data = new AuthResponseDto
                    {
                        Token = token,
                        Nombre = nombre,
                        Correo = correo,
                        Rol = rol,
                        IdUsuario = idUsuario
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        // ============================================================
        // GENERAR JWT
        // ============================================================
        private string GenerarToken(int idUsuario, string nombre, string correo, string rol)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   idUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, correo),
                new Claim(ClaimTypes.Name,               nombre),
                new Claim(ClaimTypes.Role,               rol),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                                        Convert.ToDouble(_config["Jwt:ExpirationHours"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // POST api/auth/google
            [HttpPost("google")]    
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleAuthDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdToken))
            {
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "IdToken es obligatorio."
                });
            }
 
        try
        {
        // 1. Validar el token con Google
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _config["Google:ClientId"] }
        };
 
        GoogleJsonWebSignature.Payload payload;
 
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
        }
        catch
        {
            return Unauthorized(new RespuestaDto
            {
                Exito   = false,
                Mensaje = "Token de Google inválido o expirado."
            });
        }
 
        // 2. Buscar si el usuario ya existe por correo
        var conn = (OracleConnection)_context.Database.GetDbConnection();
        await conn.OpenAsync();
 
        try
        {
            // Buscar usuario existente
            using var cmdBuscar = conn.CreateCommand();
            cmdBuscar.CommandText = "SELECT ID, NOMBRE, CORREO, ROL FROM VACUNAS.USUARIO WHERE CORREO = :correo AND ACTIVO = 1";
            cmdBuscar.Parameters.Add("correo", OracleDbType.Varchar2).Value = payload.Email;
 
            int    idUsuario = -1;
            string nombre   = payload.Name ?? payload.Email;
            string correo   = payload.Email;
            string rol      = "usuario";
 
            using var reader = await cmdBuscar.ExecuteReaderAsync();
 
            if (await reader.ReadAsync())
            {
                // Usuario existe — leer datos
                idUsuario = Convert.ToInt32(reader["ID"]);
                nombre    = reader["NOMBRE"].ToString()!;
                rol       = reader["ROL"].ToString()!;
            }
            else
            {
                // 3. Usuario nuevo — registrarlo automáticamente
                reader.Close();
 
                using var cmdRegistrar = conn.CreateCommand();
                cmdRegistrar.CommandText = "sp_registrar_usuario";
                cmdRegistrar.CommandType = System.Data.CommandType.StoredProcedure;
 
                cmdRegistrar.Parameters.Add("p_nombre",   OracleDbType.Varchar2).Value = nombre;
                cmdRegistrar.Parameters.Add("p_correo",   OracleDbType.Varchar2).Value = correo;
                // Password aleatorio hasheado — usuario Google nunca lo usa
                cmdRegistrar.Parameters.Add("p_password", OracleDbType.Varchar2).Value =
                    BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
                cmdRegistrar.Parameters.Add("p_telefono", OracleDbType.Varchar2).Value = DBNull.Value;
                cmdRegistrar.Parameters.Add("p_rol",      OracleDbType.Varchar2).Value = "usuario";
 
                var pIdOut = new OracleParameter("p_id_out", OracleDbType.Int32)
                    { Direction = System.Data.ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                    { Direction = System.Data.ParameterDirection.Output };
 
                cmdRegistrar.Parameters.Add(pIdOut);
                cmdRegistrar.Parameters.Add(pMensaje);
 
                await cmdRegistrar.ExecuteNonQueryAsync();
 
                idUsuario = Convert.ToInt32(pIdOut.Value.ToString());
 
                if (idUsuario == -1)
                    return Conflict(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = pMensaje.Value.ToString()!
                    });
            }
 
            // 4. Generar JWT propio igual que en login normal
            var token = GenerarToken(idUsuario, nombre, correo, rol);
 
            return Ok(new RespuestaDto
            {
                Exito   = true,
                Mensaje = "Login con Google exitoso.",
                Data    = new AuthResponseDto
                {
                    Token     = token,
                    Nombre    = nombre,
                    Correo    = correo,
                    Rol       = rol,
                    IdUsuario = idUsuario
                }
            });
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, new RespuestaDto
        {
            Exito   = false,
            Mensaje = "Error interno: " + ex.Message
        });
    }
}

    // ============================================================
    // Post para registrar usuarios
    // solo un admin puede crear otro admin 
    // POST api/auth/registro-admin
    // ============================================================
    [HttpPost("registro-admin")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RegistroAdmin([FromBody] RegistroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Correo) ||
            string.IsNullOrWhiteSpace(dto.Password) ||
            string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return BadRequest(new RespuestaDto
            {
                Exito = false,
                Mensaje = "Nombre, correo y password son obligatorios."
            });
        }

        // BCrypt hashea el password antes de mandarlo a Oracle
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var conn = (OracleConnection)_context.Database.GetDbConnection();

        try
        {
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_registrar_usuario";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_nombre", OracleDbType.Varchar2).Value = dto.Nombre;
            cmd.Parameters.Add("p_correo", OracleDbType.Varchar2).Value = dto.Correo;
            cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = passwordHash;
            cmd.Parameters.Add("p_telefono", OracleDbType.Varchar2).Value =
                dto.Telefono ?? (object)DBNull.Value;
            cmd.Parameters.Add("p_rol", OracleDbType.Varchar2).Value =
                "admin"; // Forzar rol admin

            var pIdOut = new OracleParameter("p_id_out", OracleDbType.Int32)
            { Direction = ParameterDirection.Output };
            var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
            { Direction = ParameterDirection.Output };

            cmd.Parameters.Add(pIdOut);
            cmd.Parameters.Add(pMensaje);

            await cmd.ExecuteNonQueryAsync();

            var idOut = Convert.ToInt32(pIdOut.Value.ToString());
            var mensaje = pMensaje.Value.ToString()!;

            if (idOut == -1)
            {
                return Conflict(new RespuestaDto { Exito = false, Mensaje = mensaje });
            }

            return Ok(new RespuestaDto
            {
                Exito = true,
                Mensaje = mensaje,
                Data = new { IdUsuario = idOut }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new RespuestaDto
            {
                Exito = false
                ,Mensaje = "Error interno: " + ex.Message
            });
        }
        finally
        {
            await conn.CloseAsync();
        }      
    }
  }
}