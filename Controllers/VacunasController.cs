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
    public class VacunasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VacunasController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET api/vacunas
        // Lista todas las vacunas activas
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerVacunas([FromQuery] string? tipo)
        {
            try
            {
                var query = _context.Vacunas
                    .Where(v => v.Activo) ;

                // Filtro opcional por tipo: humano | animal | ambos
                if (!string.IsNullOrWhiteSpace(tipo))
                    query = query.Where(v => v.Tipo == tipo);

                var vacunas = await query
                    .OrderBy(v => v.Nombre)
                    .Select(v => new VacunaDto
                    {
                        Id          = v.Id,
                        Nombre      = v.Nombre,
                        Tipo        = v.Tipo,
                        Fabricante  = v.Fabricante,
                        Descripcion = v.Descripcion,
                        ImagenUrl   = v.ImagenUrl,
                        Activo      = v.Activo
                    })
                    .ToListAsync();

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
        // GET api/vacunas/{id}
        // Obtiene una vacuna con su esquema de dosis
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerVacuna(int id)
        {
            try
            {
                var vacuna = await _context.Vacunas
                    .Where(v => v.Id == id && v.Activo == true)
                    .Select(v => new VacunaDto
                    {
                        Id          = v.Id,
                        Nombre      = v.Nombre,
                        Tipo        = v.Tipo,
                        Fabricante  = v.Fabricante,
                        Descripcion = v.Descripcion,
                        ImagenUrl   = v.ImagenUrl,
                        Activo      = v.Activo
                    })
                    .FirstOrDefaultAsync();

                if (vacuna == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Vacuna no encontrada."
                    });

                var esquemas = await _context.Esquemavacunas
                    .Where(e => e.IdVacuna == id)
                    .Select(e => new EsquemaVacunaDto
                    {
                        Id             = e.Id,
                        IdVacuna       = e.IdVacuna,
                        NumeroDosis    = e.NumeroDosis,
                        IntervaloDias  = e.IntervaloDias,
                        EdadMinimaDias = e.EdadMinimaDias,
                        Descripcion    = e.Descripcion
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = new VacunaConEsquemaDto
                    {
                        Vacuna   = vacuna,
                        Esquemas = esquemas
                    }
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
        // POST api/vacunas
        // Crear vacuna — solo admin
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CrearVacuna([FromBody] CrearVacunaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Tipo))
            {
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Nombre y tipo son obligatorios."
                });
            }

            if (dto.Tipo != "humano" && dto.Tipo != "animal" && dto.Tipo != "ambos")
            {
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Tipo debe ser humano, animal o ambos."
                });
            }

            try
            {
                var vacuna = new Vacuna
                {
                    Nombre      = dto.Nombre,
                    Tipo        = dto.Tipo,
                    Fabricante  = dto.Fabricante,
                    Descripcion = dto.Descripcion,
                    ImagenUrl   = dto.ImagenUrl,
                    Activo      = true
                };

                _context.Vacunas.Add(vacuna);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Vacuna creada correctamente.",
                    Data    = new { IdVacuna = vacuna.Id }
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
        // PUT api/vacunas/{id}
        // Actualizar vacuna — solo admin
        // ============================================================
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ActualizarVacuna(int id, [FromBody] ActualizarVacunaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Tipo))
            {
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Nombre y tipo son obligatorios."
                });
            }

            try
            {
                var vacuna = await _context.Vacunas
                    .FirstOrDefaultAsync(v => v.Id == id && v.Activo == true);

                if (vacuna == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Vacuna no encontrada."
                    });

                vacuna.Nombre      = dto.Nombre;
                vacuna.Tipo        = dto.Tipo;
                vacuna.Fabricante  = dto.Fabricante;
                vacuna.Descripcion = dto.Descripcion;
                vacuna.ImagenUrl   = dto.ImagenUrl;

                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Vacuna actualizada correctamente."
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
        // DELETE api/vacunas/{id}
        // Desactivar vacuna — solo admin
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DesactivarVacuna(int id)
        {
            try
            {
                var vacuna = await _context.Vacunas
                    .FirstOrDefaultAsync(v => v.Id == id && v.Activo == true);

                if (vacuna == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Vacuna no encontrada."
                    });

                vacuna.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Vacuna desactivada correctamente."
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
        // POST api/vacunas/esquema
        // Agregar esquema de dosis a una vacuna — solo admin
        // ============================================================
        [HttpPost("esquema")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CrearEsquema([FromBody] CrearEsquemaDto dto)
        {
            if (dto.IdVacuna == 0 || dto.NumeroDosis == 0)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "IdVacuna y NumeroDosis son obligatorios."
                });
            }

            try
            {
                var esquema = new Esquemavacuna
                {
                    IdVacuna       = dto.IdVacuna,
                    NumeroDosis    = dto.NumeroDosis,
                    IntervaloDias  = dto.IntervaloDias,
                    EdadMinimaDias = dto.EdadMinimaDias,
                    Descripcion    = dto.Descripcion
                };

                _context.Esquemavacunas.Add(esquema);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Esquema creado correctamente.",
                    Data    = new { IdEsquema = esquema.Id }
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