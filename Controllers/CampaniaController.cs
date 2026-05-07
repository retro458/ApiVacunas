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

    public class CampaniaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CampaniaController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: api/campanias
        // OBTENER TODAS LAS CAMPAÑAS ACTIVAS
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CampaniaDto>>> GetCampanias()
        {

            try
            {
                var campanias = await _context.Campaniavacunacions
                    .Where(c => c.Activo)
                    .Select(c => new CampaniaDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Lugar = c.Lugar,
                        Fecha = c.Fecha,
                        Latitud = c.Latitud,
                        Longitud = c.Longitud,
                        IdVacuna = c.IdVacuna,
                        IdCentro = c.IdCentro,
                        Activo = c.Activo
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Campañas obtenidas exitosamente",
                    Data = campanias
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error al obtener campañas: " + ex.Message
                });
            }
        }


        // ============================================================
        // POST: api/campanias -> CREAR NUEVA CAMPAÑA, SOLO PARA ADMINISTRADORES
        // ============================================================
        [HttpPost]
[Authorize(Roles = "admin")]
public async Task<ActionResult> CrearCampania([FromBody] CrearCampaniaDto dto)
{
    try
    {
        int? centroIdFinal = dto.IdCentro;
        decimal? lat = dto.Latitud;
        decimal? lng = dto.Longitud;

        // 1. Lógica para crear el Centro Móvil si no viene ID
        if (!dto.IdCentro.HasValue && dto.Latitud.HasValue)
        {
            var nuevoMovel = new Centrovacunacion {
                Nombre = $"Punto Móvil: {dto.Lugar}",
                Direccion = dto.Lugar,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                Tipo = "MÓVIL",
                Activo = true
            };
            _context.Centrovacunacions.Add(nuevoMovel);
            await _context.SaveChangesAsync(); // Oracle genera el ID
            centroIdFinal = nuevoMovel.Id;
        }
        else if (dto.IdCentro.HasValue)
        {
            // Heredar coordenadas del centro fijo para la campaña
            var centro = await _context.Centrovacunacions.FindAsync(dto.IdCentro.Value);
            if (centro != null) { lat = centro.Latitud; lng = centro.Longitud; }
        }

        // 2. Crear la Campaña (El trigger se activará al hacer SaveChanges)
        var nuevaCampania = new Campaniavacunacion {
            Nombre = dto.Nombre,
            Lugar = dto.Lugar,
            Fecha = dto.Fecha,
            Latitud = lat,
            Longitud = lng,
            IdVacuna = dto.IdVacuna,
            IdCentro = centroIdFinal, // El trigger trg_campania_centro usará este ID
            Activo = true
        };

        _context.Campaniavacunacions.Add(nuevaCampania);
        await _context.SaveChangesAsync(); 

        return Ok(new RespuestaDto { 
            Exito = true, 
            Mensaje = "Campaña creada (Vínculo generado por DB)", 
            Data = nuevaCampania.Id 
        });
    }
    catch (Exception ex) {
        return StatusCode(500, new RespuestaDto { Exito = false, Mensaje = ex.Message });
    }
}

        // ============================================================
        // PUT: api/campanias/{id} -> ACTUALIZAR CAMPAÑA EXISTENTE, SOLO PARA ADMINISTRADORES
        // ============================================================
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> ActualizarCampania(int id, [FromBody] ActualizarCampaniaDto dto)
        {
            try
            {
                var campaniaExistente = await _context.Campaniavacunacions
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (campaniaExistente == null)
                {
                    return NotFound(new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Campaña no encontrada"
                    });
                }

                decimal? latitud = dto.Latitud;
                decimal? longitud = dto.Longitud;

                if (dto.IdCentro.HasValue)
                {
                    var centro = await _context.Centrovacunacions
                    .FirstOrDefaultAsync(c => c.Id == dto.IdCentro.Value);

                    if (centro != null)
                    {
                        latitud = centro.Latitud;
                        longitud = centro.Longitud;
                    }
                }

                campaniaExistente.Nombre = dto.Nombre;
                campaniaExistente.Lugar = dto.Lugar;
                campaniaExistente.Fecha = dto.Fecha;
                campaniaExistente.Latitud = latitud;
                campaniaExistente.Longitud = longitud;
                campaniaExistente.IdVacuna = dto.IdVacuna;
                campaniaExistente.IdCentro = dto.IdCentro;
                campaniaExistente.Activo = dto.Activo ?? campaniaExistente.Activo;

                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Campaña actualizada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error al actualizar campaña: " + ex.Message
                });
            }


        }

        // ============================================================
        // DELETE: api/campanias/{id} -> ELIMINAR CAMPAÑA (SOFT DELETE), SOLO PARA ADMINISTRADORES
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> EliminarCampania(int id)
        {
            try
            {
                var campaniaExistente = await _context.Campaniavacunacions
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (campaniaExistente == null)
                {
                    return NotFound(new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Campaña no encontrada"
                    });
                }

                campaniaExistente.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Campaña eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error al eliminar campaña: " + ex.Message
                });
            }
        }
    }
}