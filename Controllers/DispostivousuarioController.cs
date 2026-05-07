using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using ApiVacunas.Models;

namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DispositivoUsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DispositivoUsuarioController(AppDbContext context)
        {
            _context = context;
        }

        private int ObtenerIdUsuario() =>
            Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt
                            .JwtRegisteredClaimNames.Sub));

        // ============================================================
        // GET api/dispositivousuario
        // Dispositivos registrados del usuario autenticado
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerDispositivos()
        {
            var idUsuario = ObtenerIdUsuario();

            try
            {
                var dispositivos = await _context.Dispositivousuarios
                    .Where(d => d.IdUsuario == idUsuario)
                    .OrderByDescending(d => d.FechaRegistro)
                    .Select(d => new DispositivoDto
                    {
                        Id            = d.Id,
                        TokenPush     = d.TokenPush,
                        Plataforma    = d.Plataforma,
                        Activo        = d.Activo == true,
                        FechaRegistro = d.FechaRegistro
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = dispositivos
                });
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
        // POST api/dispositivousuario
        // Registrar token push del dispositivo
        // Android lo llama al iniciar sesión para habilitar notificaciones
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> RegistrarDispositivo([FromBody] RegistrarDispositivoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TokenPush) ||
                string.IsNullOrWhiteSpace(dto.Plataforma))
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "TokenPush y Plataforma son obligatorios."
                });

            if (dto.Plataforma != "android" && dto.Plataforma != "ios")
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Plataforma debe ser android o ios."
                });

            var idUsuario = ObtenerIdUsuario();

            try
            {
                // Si el token ya existe lo reactiva en lugar de duplicar
                var dispositivoExistente = await _context.Dispositivousuarios
                    .FirstOrDefaultAsync(d => d.TokenPush == dto.TokenPush
                                           && d.IdUsuario == idUsuario);

                if (dispositivoExistente != null)
                {
                    dispositivoExistente.Activo = true;
                    await _context.SaveChangesAsync();

                    return Ok(new RespuestaDto
                    {
                        Exito   = true,
                        Mensaje = "Dispositivo reactivado correctamente.",
                        Data    = new { IdDispositivo = dispositivoExistente.Id }
                    });
                }

                var dispositivo = new Dispositivousuario
                {
                    IdUsuario     = idUsuario,
                    TokenPush     = dto.TokenPush,
                    Plataforma    = dto.Plataforma,
                    Activo        = true,
                    FechaRegistro = DateTime.Now
                };

                _context.Dispositivousuarios.Add(dispositivo);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Dispositivo registrado correctamente.",
                    Data    = new { IdDispositivo = dispositivo.Id }
                });
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
        // DELETE api/dispositivousuario/{id}
        // Desactivar dispositivo — al cerrar sesión
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DesactivarDispositivo(int id)
        {
            var idUsuario = ObtenerIdUsuario();

            try
            {
                var dispositivo = await _context.Dispositivousuarios
                    .FirstOrDefaultAsync(d => d.Id == id && d.IdUsuario == idUsuario);

                if (dispositivo == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Dispositivo no encontrado."
                    });

                dispositivo.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Dispositivo desactivado correctamente."
                });
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
    }
}