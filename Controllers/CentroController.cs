using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using ApiVacunas.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;


namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class CentroController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CentroController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: api/centros
        // OBTENER TODOS LOS CENTROS
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CentroDto>>> GetCentros()
        {

            try
            {
                var centros = await _context.Centrovacunacions
                    .Where(c => c.Activo)
                    .Select(c => new CentroDto
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Direccion = c.Direccion,
                        Latitud = c.Latitud,
                        Longitud = c.Longitud,
                        Tipo = c.Tipo,
                        Horario = c.Horario,
                        Telefono = c.Telefono,
                        Activo = c.Activo
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Centros obtenidos exitosamente",
                    Data = centros
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = $"Error al obtener centros: {ex.Message}"
                });

            }

        }

        // ============================================================
        // POST: api/centros -> CREAR UN NUEVO CENTRO, REQUIERE ROL ADMIN
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> CrearCentro([FromBody] CrearCentroDto crearCentroDto)
        {
            try
            {
                var nuevoCentro = new Centrovacunacion
                {
                    Nombre = crearCentroDto.Nombre,
                    Direccion = crearCentroDto.Direccion,
                    Latitud = crearCentroDto.Latitud,
                    Longitud = crearCentroDto.Longitud,
                    Tipo = crearCentroDto.Tipo,
                    Horario = crearCentroDto.Horario,
                    Telefono = crearCentroDto.Telefono,
                    Activo = true
                };

                _context.Centrovacunacions.Add(nuevoCentro);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Centro creado exitosamente",
                    Data = nuevoCentro.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = $"Error al crear centro: {ex.Message}"
                });
            }

        }

        // ============================================================
        // PUT: api/centros/{id} -> ACTUALIZAR UN CENTRO EXISTENTE, REQUIERE ROL ADMIN
        // ============================================================
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]

        public async Task<ActionResult> ActualizarCentro(int id, [FromBody] ActualizarCentroDto actualizarCentroDto)
        {
            try
            {
                var centroExistente = await _context.Centrovacunacions.FindAsync(id);

                if (centroExistente == null)
                {
                    return NotFound(new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Centro no encontrado"
                    });
                }

                centroExistente.Nombre = actualizarCentroDto.Nombre;
                centroExistente.Direccion = actualizarCentroDto.Direccion;
                centroExistente.Latitud = actualizarCentroDto.Latitud;
                centroExistente.Longitud = actualizarCentroDto.Longitud;
                centroExistente.Tipo = actualizarCentroDto.Tipo;
                centroExistente.Horario = actualizarCentroDto.Horario;
                centroExistente.Telefono = actualizarCentroDto.Telefono;
                centroExistente.Activo = actualizarCentroDto.Activo;

                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Centro actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = $"Error al actualizar centro: {ex.Message}"
                });
            }
        }

        // ============================================================
        // DELETE: api/centros/{id} -> ELIMINAR UN CENTRO (SOFT DELETE), REQUIERE ROL ADMIN
        // ============================================================
        [HttpDelete]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> EliminarCentro(int id)
        {
            try
            {
                var centroExistente = await _context.Centrovacunacions.FindAsync(id);

                if (centroExistente == null)
                {
                    return NotFound(new RespuestaDto
                    {
                        Exito = false,
                        Mensaje = "Centro no encontrado"
                    });
                }

                // Soft delete: marcar como inactivo en lugar de eliminar físicamente
                centroExistente.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Centro eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = $"Error al eliminar centro: {ex.Message}"
                });
            }
        }
        // ============================================================
        // GET api/centros/{id}/detalle
        // Centro con todas sus vacunas disponibles
        // ============================================================
 
[HttpGet("{id}/detalle")]
public async Task<IActionResult> ObtenerDetalleCentro(int id)
{
    try
    {
        var centro = await _context.Centrovacunacions
            .FirstOrDefaultAsync(c => c.Id == id);
 
        if (centro == null)
            return NotFound(new RespuestaDto
            {
                Exito   = false,
                Mensaje = "Centro no encontrado."
            });
 
        var vacunas = await (
            from vc in _context.Vacunaencentros
            join v in _context.Vacunas on vc.IdVacuna equals v.Id
            where vc.IdCentro == id
            orderby v.Nombre
            select new VacunaCentroDto
            {
                Id         = vc.Id,
                IdCentro   = centro.Id,
                Centro     = centro.Nombre,
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
            Data    = new
            {
                Centro = new
                {
                    centro.Id,
                    centro.Nombre,
                    centro.Direccion,
                    centro.Latitud,
                    centro.Longitud,
                    centro.Tipo,
                    centro.Horario,
                    centro.Telefono
                },
                Vacunas = vacunas
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

        // GET api/centros/cercanos
[HttpGet("cercanos")]
public async Task<IActionResult> ObtenerCentrosCercanos(
    [FromQuery] double latitud,
    [FromQuery] double longitud,
    [FromQuery] double radioKm = 5)
{
    var conn = (OracleConnection)_context.Database.GetDbConnection();

    try
    {
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "sp_centros_cercanos";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        cmd.Parameters.Add("p_latitud",  OracleDbType.Decimal).Value = latitud;
        cmd.Parameters.Add("p_longitud", OracleDbType.Decimal).Value = longitud;
        cmd.Parameters.Add("p_radio_km", OracleDbType.Decimal).Value = radioKm;

        var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
            { Direction = System.Data.ParameterDirection.Output };
        var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
            { Direction = System.Data.ParameterDirection.Output };

        cmd.Parameters.Add(pCursor);
        cmd.Parameters.Add(pMensaje);

        await cmd.ExecuteNonQueryAsync();

        var centros = new List<CentroDto>();
        using var reader = ((OracleRefCursor)pCursor.Value).GetDataReader();

        while (await reader.ReadAsync())
        {
            centros.Add(new CentroDto
            {
                Id           = Convert.ToInt32(reader["ID"]),
                Nombre       = reader["NOMBRE"].ToString()!,
                Direccion    = reader["DIRECCION"] == DBNull.Value
                                ? null : reader["DIRECCION"].ToString(),
                Latitud      = Convert.ToDecimal(reader["LATITUD"]),
                Longitud     = Convert.ToDecimal(reader["LONGITUD"]),
                Tipo         = reader["TIPO"] == DBNull.Value
                                ? null : reader["TIPO"].ToString(),
                Horario      = reader["HORARIO"] == DBNull.Value
                                ? null : reader["HORARIO"].ToString(),
                Telefono     = reader["TELEFONO"] == DBNull.Value
                                ? null : reader["TELEFONO"].ToString(),
                DistanciaKm  = Convert.ToDecimal(reader["DISTANCIA_KM"])
            });
        }

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
    finally
    {
        await conn.CloseAsync();
    }
}
    }
}