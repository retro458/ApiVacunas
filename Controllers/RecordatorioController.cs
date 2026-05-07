using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using ApiVacunas.Models;
using System.IdentityModel.Tokens.Jwt;

namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecordatoriosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecordatoriosController(AppDbContext context)
        {
            _context = context;
        }

       // Método corregido para obtener el ID del usuario
        private int ObtenerIdUsuario()
        {
            
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(idClaim))
            {
              //  Console.WriteLine("ADVERTENCIA: No se encontró el claim NameIdentifier");
                return 0;
            }
            
            if (int.TryParse(idClaim, out int id))
                return id;
            
             // Console.WriteLine($"ERROR: No se pudo convertir '{idClaim}' a entero");
            return 0;
        }

        // ============================================================
        // GET api/recordatorios
        // Todos los recordatorios de vacunas pendientes del usuario
        // ============================================================
      [HttpGet]
        public async Task<IActionResult> ObtenerRecordatorios([FromQuery] string? estado)
        {
            try
            {
                var idUsuario = ObtenerIdUsuario();
                
                if (idUsuario == 0)
                {
                    return Unauthorized(new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Usuario no autenticado o token inválido."
                    });
                }

                var query = from r in _context.Recordatorios
                            join h in _context.Historialvacunas on r.IdHistorial equals h.Id
                            join m in _context.Miembros on h.IdMiembro equals m.Id
                            join v in _context.Vacunas on h.IdVacuna equals v.Id
                            where m.IdUsuario == idUsuario && m.Activo == true
                            select new RecordatorioDto
                            {
                                Id = r.Id,
                                IdHistorial = r.IdHistorial,
                                FechaRecordatorio = r.FechaRecordatorio,
                                Tipo = r.Tipo,
                                Mensaje = r.Mensaje,
                                Estado = r.Estado,
                                Vacuna = v.Nombre,
                                Miembro = m.Nombre,
                                DiasRestantes = (int?)(r.FechaRecordatorio - DateTime.Today).TotalDays
                            };

                // Filtro opcional por estado
                if (!string.IsNullOrWhiteSpace(estado))
                    query = query.Where(r => r.Estado == estado);

                var recordatorios = await query
                    .OrderBy(r => r.FechaRecordatorio)
                    .ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "OK",
                    Data = recordatorios
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en ObtenerRecordatorios: {ex.Message}");
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
        }


       
        // ============================================================
        // GET api/recordatorios/proximos
        // Recordatorios de los próximos 30 días
        // ============================================================
        [HttpGet("proximos")]
        public async Task<IActionResult> ObtenerProximos([FromQuery] int dias = 30)
        {
            
            

            try
            {
                    var idUsuario  = ObtenerIdUsuario();
                    if(idUsuario == 0)
                    {
                        return Unauthorized(new RespuestaDto
                        {
                            Exito   = false,
                            Mensaje = "Usuario no autenticado o token inválido."
                        });
                    }
                    var fechaLimite = DateTime.Today.AddDays(dias);


                var recordatorios = await (
                    from r in _context.Recordatorios
                    join h in _context.Historialvacunas on r.IdHistorial equals h.Id
                    join m in _context.Miembros on h.IdMiembro equals m.Id
                    join v in _context.Vacunas  on h.IdVacuna  equals v.Id
                    where m.IdUsuario         == idUsuario
                       && m.Activo           == true
                       && r.Estado           == "pendiente"
                       && r.FechaRecordatorio >= DateTime.Today
                       && r.FechaRecordatorio <= fechaLimite
                    orderby r.FechaRecordatorio
                    select new RecordatorioDto
                    {
                        Id                = r.Id,
                        IdHistorial       = r.IdHistorial,
                        FechaRecordatorio = r.FechaRecordatorio,
                        Tipo              = r.Tipo,
                        Mensaje           = r.Mensaje,
                        Estado            = r.Estado,
                        Vacuna            = v.Nombre,
                        Miembro           = m.Nombre,
                        DiasRestantes     = (int?)(r.FechaRecordatorio - DateTime.Today).TotalDays
                    }
                ).ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = recordatorios
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
        // GET api/recordatorios/campanias
        // Recordatorios de campañas de vacunación próximas
        // ============================================================
        [HttpGet("campanias")]
        public async Task<IActionResult> ObtenerRecordatoriosCampanias([FromQuery] int dias = 30)
        {
            var fechaLimite = DateTime.Today.AddDays(dias);

            try
            {
                var campanias = await (
                    from c in _context.Campaniavacunacions
                    join v in _context.Vacunas on c.IdVacuna equals v.Id into vj
                    from v in vj.DefaultIfEmpty()
                    where c.Activo == true
                       && c.Fecha  >= DateTime.Today
                       && c.Fecha  <= fechaLimite
                    orderby c.Fecha
                    select new RecordatorioCampaniaDto
                    {
                        Id                = c.Id,
                        Campania          = c.Nombre,
                        Vacuna            = v != null ? v.Nombre : null,
                        Lugar             = c.Lugar,
                        FechaCampania     = c.Fecha,
                        FechaRecordatorio = c.Fecha,
                        DiasRestantes     = (int?)(c.Fecha - DateTime.Today).TotalDays
                    }
                ).ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = campanias
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
        // PATCH api/recordatorios/{id}/estado
        // Actualizar estado de un recordatorio
        // pendiente | completado | pospuesto
        // ============================================================
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarRecordatorioDto dto)
        {
            var estadosValidos = new[] { "pendiente", "completado", "pospuesto" };

            if (!estadosValidos.Contains(dto.Estado))
            {
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Estado debe ser pendiente, completado o pospuesto."
                });
            }

            try
            {
                var recordatorio = await _context.Recordatorios
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recordatorio == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Recordatorio no encontrado."
                    });

                recordatorio.Estado = dto.Estado;
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Estado actualizado correctamente."
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