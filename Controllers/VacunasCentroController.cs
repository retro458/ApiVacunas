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
    public class VacunasEnCentroController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VacunasEnCentroController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET api/vacunasencentro/centro/{idCentro}
        // Vacunas disponibles en un centro específico
        // ============================================================
        [HttpGet("centro/{idCentro}")]
        public async Task<IActionResult> ObtenerVacunasPorCentro(int idCentro)
        {
            try
            {
                var vacunas = await (
                    from vc in _context.Vacunaencentros
                    join v  in _context.Vacunas           on vc.IdVacuna equals v.Id
                    join c  in _context.Centrovacunacions on vc.IdCentro equals c.Id
                    where vc.IdCentro == idCentro
                    orderby v.Nombre
                    select new VacunaCentroDto
                    {
                        Id         = vc.Id,
                        IdCentro   = c.Id,
                        Centro     = c.Nombre,
                        IdVacuna   = v.Id,
                        Vacuna     = v.Nombre,
                        Fabricante = v.Fabricante,
                        TipoVacuna = v.Tipo,
                        Disponible = vc.Disponible,
                        Precio     = vc.Precio
                    }
                ).ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = vacunas
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
        // GET api/vacunasencentro/vacuna/{idVacuna}
        // Centros donde está disponible una vacuna específica
        // ============================================================
        [HttpGet("vacuna/{idVacuna}")]
        public async Task<IActionResult> ObtenerCentrosPorVacuna(int idVacuna)
        {
            try
            {
                var centros = await (
                    from vc in _context.Vacunaencentros
                    join v  in _context.Vacunas           on vc.IdVacuna equals v.Id
                    join c  in _context.Centrovacunacions on vc.IdCentro equals c.Id
                    where vc.IdVacuna    == idVacuna
                       && vc.Disponible == true
                       && c.Activo      == true
                    orderby c.Nombre
                    select new VacunaCentroDto
                    {
                        Id         = vc.Id,
                        IdCentro   = c.Id,
                        Centro     = c.Nombre,
                        IdVacuna   = v.Id,
                        Vacuna     = v.Nombre,
                        Fabricante = v.Fabricante,
                        TipoVacuna = v.Tipo,
                        Disponible = vc.Disponible,
                        Precio     = vc.Precio
                    }
                ).ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = centros
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
        // POST api/vacunasencentro
        // Asignar vacuna a un centro — solo admin
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AsignarVacuna([FromBody] AsignarVacunaCentroDto dto)
        {
            if (dto.IdCentro == 0 || dto.IdVacuna == 0)
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "IdCentro e IdVacuna son obligatorios."
                });

            try
            {
                // Verificar que no esté ya asignada
                var yaExiste = await _context.Vacunaencentros
                .Where(vc => vc.IdCentro == dto.IdCentro && vc.IdVacuna == dto.IdVacuna)
                .FirstOrDefaultAsync();

                if (yaExiste! != null)
                    return Conflict(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Esta vacuna ya está asignada a ese centro."
                    });

                var vacunaCentro = new Vacunaencentro
                {
                    IdCentro   = dto.IdCentro,
                    IdVacuna   = dto.IdVacuna,
                    Disponible = dto.Disponible,
                    Precio     = dto.Precio
                };

                _context.Vacunaencentros.Add(vacunaCentro);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Vacuna asignada al centro correctamente.",
                    Data    = new { IdVacunaCentro = vacunaCentro.Id }
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
        // PATCH api/vacunasencentro/{id}
        // Actualizar disponibilidad y precio — solo admin
        // ============================================================
        [HttpPatch("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ActualizarDisponibilidad(
            int id, [FromBody] ActualizarVacunaCentroDto dto)
        {
            try
            {
                var vacunaCentro = await _context.Vacunaencentros
                    .FirstOrDefaultAsync(vc => vc.Id == id);

                if (vacunaCentro == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Asignación no encontrada."
                    });

                vacunaCentro.Disponible = dto.Disponible;
                vacunaCentro.Precio     = dto.Precio;

                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Disponibilidad actualizada correctamente."
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
        // DELETE api/vacunasencentro/{id}
        // Quitar vacuna de un centro — solo admin
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> QuitarVacuna(int id)
        {
            try
            {
                var vacunaCentro = await _context.Vacunaencentros
                    .FirstOrDefaultAsync(vc => vc.Id == id);

                if (vacunaCentro == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Asignación no encontrada."
                    });

                _context.Vacunaencentros.Remove(vacunaCentro);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Vacuna quitada del centro correctamente."
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