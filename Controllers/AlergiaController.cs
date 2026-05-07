using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using ApiVacunas.Models;

namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlergiasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AlergiasController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET api/alergias
        // Catálogo completo de alergias disponibles
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerAlergias()
        {
            try
            {
                var alergias = await _context.Alergia
                    .OrderBy(a => a.Nombre)
                    .Select(a => new AlergiaDto
                    {
                        Id     = a.Id,
                        Nombre = a.Nombre!
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = alergias
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
        // POST api/alergias
        // Agregar nueva alergia al catálogo — solo admin
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CrearAlergia([FromBody] CrearAlergiaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "El nombre es obligatorio."
                });

            try
            {
                // Verificar si ya existe
                var existe = await _context.Alergia
                    .FirstOrDefaultAsync(a => a.Nombre!.ToUpper() == dto.Nombre.ToUpper()) != null;

                if (existe)
                    return Conflict(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Ya existe una alergia con ese nombre."
                    });

                var alergia = new Alergia { Nombre = dto.Nombre };

                _context.Alergia.Add(alergia);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Alergia creada correctamente.",
                    Data    = new { IdAlergia = alergia.Id }
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
        // DELETE api/alergias/{id}
        // Eliminar alergia del catálogo — solo admin
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EliminarAlergia(int id)
        {
            try
            {
                var alergia = await _context.Alergia
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (alergia == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Alergia no encontrada."
                    });

                // Verificar si está asignada a algún miembro
                var enUso = await _context.Miembroalergia
                    .FirstOrDefaultAsync(ma => ma.IdAlergia == id) != null ;

                if (enUso)
                    return Conflict(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "No se puede eliminar, está asignada a uno o más miembros."
                    });

                _context.Alergia.Remove(alergia);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Alergia eliminada correctamente."
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
        // GET api/alergias/miembro/{idMiembro}
        // Alergias asignadas a un miembro específico
        // ============================================================
        [HttpGet("miembro/{idMiembro}")]
        public async Task<IActionResult> ObtenerAlergiasMiembro(int idMiembro)
        {
            try
            {
                var alergias = await (
                    from ma in _context.Miembroalergia
                    join a in _context.Alergia on ma.IdAlergia equals a.Id
                    where ma.IdMiembro == idMiembro
                    orderby a.Nombre
                    select new MiembroAlergiaDto
                    {
                        IdMiembroAlergia = ma.Id,
                        IdAlergia        = a.Id,
                        Nombre           = a.Nombre!
                    }
                ).ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = alergias
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
        // POST api/alergias/miembro/{idMiembro}
        // Asignar alergia a un miembro
        // ============================================================
        [HttpPost("miembro/{idMiembro}")]
        public async Task<IActionResult> AsignarAlergia(int idMiembro, [FromBody] AsignarAlergiaDto dto)
        {
            try
            {
                // Verificar que el miembro existe
                var miembro = await _context.Miembros
                    .FirstOrDefaultAsync(m => m.Id == idMiembro);

                if (miembro == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Miembro no encontrado."
                    });

                if (!miembro.Activo)
                    return BadRequest(new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "No se pueden asignar alergias a un miembro inactivo."
                    });
                // Verificar que la alergia existe
                var alergiaExiste = await _context.Alergia
                    .FirstOrDefaultAsync(a => a.Id == dto.IdAlergia);

                if (alergiaExiste == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Alergia no encontrada."
                    });

                // Verificar que no esté ya asignada (constraint UNIQUE en BD)
                var yaAsignada = await _context.Miembroalergia
                    .FirstOrDefaultAsync(ma => ma.IdMiembro == idMiembro
                                            && ma.IdAlergia == dto.IdAlergia) != null;

                if (yaAsignada)
                    return Conflict(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Esta alergia ya está asignada al miembro."
                    });

                var miembroAlergia = new Miembroalergia
                {
                    IdMiembro = idMiembro,
                    IdAlergia = dto.IdAlergia
                };

                _context.Miembroalergia.Add(miembroAlergia);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Alergia asignada correctamente.",
                    Data    = new { IdMiembroAlergia = miembroAlergia.Id }
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
        // DELETE api/alergias/miembro/{idMiembro}/{idAlergia}
        // Quitar alergia de un miembro
        // ============================================================
        [HttpDelete("miembro/{idMiembro}/{idAlergia}")]
        public async Task<IActionResult> QuitarAlergia(int idMiembro, int idAlergia)
        {
            try
            {
                var miembroAlergia = await _context.Miembroalergia
                    .FirstOrDefaultAsync(ma => ma.IdMiembro == idMiembro
                                            && ma.IdAlergia == idAlergia);

                if (miembroAlergia == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Asignación no encontrada."
                    });

                _context.Miembroalergia.Remove(miembroAlergia);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Alergia quitada correctamente."
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