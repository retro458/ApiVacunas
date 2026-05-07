using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using ApiVacunas.Models;
using Oracle.ManagedDataAccess.Types;

namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImcController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ImcController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET api/imc/{idMiembro}
        // Historial IMC de un miembro con clasificación
        // Usa el SP que devuelve la columna virtual calculada por Oracle
        // ============================================================
        [HttpGet("{idMiembro}")]
        public async Task<IActionResult> ObtenerHistorialImc(int idMiembro)
        {
            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_historial_imc";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id_miembro", OracleDbType.Int32).Value = idMiembro;

                var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                    { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                    { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCursor);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var historial = new List<ImcDto>();

                using var reader = ((OracleRefCursor)pCursor.Value).GetDataReader();

                while (await reader.ReadAsync())
                {
                    historial.Add(new ImcDto
                    {
                        Id            = Convert.ToInt32(reader["ID"]),
                        IdMiembro     = idMiembro,
                        Peso          = Convert.ToDecimal(reader["PESO"]),
                        Altura        = Convert.ToDecimal(reader["ALTURA"]),
                        Resultado     = reader["RESULTADO"] == DBNull.Value
                                        ? null : Convert.ToDecimal(reader["RESULTADO"]),
                        Clasificacion = reader["CLASIFICACION"].ToString()!,
                        Fecha         = Convert.ToDateTime(reader["FECHA"])
                    });
                }

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = historial
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

        // ============================================================
        // POST api/imc
        // Registrar nuevo IMC
        // Oracle calcula el resultado automáticamente (columna virtual)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> RegistrarImc([FromBody] RegistrarImcDto dto)
        {
            if (dto.Peso <= 0 || dto.Altura <= 0)
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Peso y altura deben ser mayores a 0."
                });

            if (dto.Altura > 3)
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Altura debe estar en metros (ej: 1.75)."
                });

            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_registrar_imc";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id_miembro", OracleDbType.Int32).Value    = dto.IdMiembro;
                cmd.Parameters.Add("p_peso",       OracleDbType.Decimal).Value  = dto.Peso;
                cmd.Parameters.Add("p_altura",     OracleDbType.Decimal).Value  = dto.Altura;
                cmd.Parameters.Add("p_fecha",      OracleDbType.Date).Value     = dto.Fecha;

                var pIdOut = new OracleParameter("p_id_out", OracleDbType.Int32)
                    { Direction = ParameterDirection.Output };
                var pResultado = new OracleParameter("p_resultado", OracleDbType.Decimal)
                    { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                    { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pIdOut);
                cmd.Parameters.Add(pResultado);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var idOut    = Convert.ToInt32(pIdOut.Value.ToString());
                var mensaje  = pMensaje.Value.ToString()!;
                var resultado = pResultado.Value == DBNull.Value
                                ? (decimal?)null
                                : Convert.ToDecimal(pResultado.Value.ToString());

                if (idOut == -1)
                    return BadRequest(new RespuestaDto { Exito = false, Mensaje = mensaje });

                // Clasificación en C# para devolverla en la respuesta inmediata
                var clasificacion = resultado switch
                {
                    < 18.5m  => "Bajo peso",
                    < 25m    => "Normal",
                    < 30m    => "Sobrepeso",
                    not null => "Obesidad",
                    _        => "Sin clasificar"
                };

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = mensaje,
                    Data    = new
                    {
                        IdImc         = idOut,
                        Resultado     = resultado,
                        Clasificacion = clasificacion
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
            finally
            {
                await conn.CloseAsync();
            }
        }

        // ============================================================
        // GET api/imc/carnet/{idMiembro}
        // Carnets escaneados de un miembro
        // ============================================================
        [HttpGet("carnet/{idMiembro}")]
        public async Task<IActionResult> ObtenerCarnets(int idMiembro)
        {
            try
            {
                var carnets = await _context.Carnetescaneados
                    .Where(c => c.IdMiembro == idMiembro)
                    .OrderByDescending(c => c.Fecha)
                    .Select(c => new CarnetEscaneadoDto
                    {
                        Id            = c.Id,
                        IdMiembro     = c.IdMiembro,
                        NombreClinica = c.NombreClinica,
                        Fecha         = c.Fecha,
                        Imagen        = c.Imagen
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = carnets
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
        // POST api/imc/carnet
        // Registrar carnet escaneado de clínica externa
        // ============================================================
        [HttpPost("carnet")]
        public async Task<IActionResult> RegistrarCarnet([FromBody] RegistrarCarnetDto dto)
        {
            if (dto.IdMiembro == 0)
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "IdMiembro es obligatorio."
                });

            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_registrar_carnet";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id_miembro",     OracleDbType.Int32).Value    = dto.IdMiembro;
                cmd.Parameters.Add("p_nombre_clinica", OracleDbType.Varchar2).Value =
                    dto.NombreClinica ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_fecha",          OracleDbType.Date).Value     =
                    dto.Fecha.HasValue ? dto.Fecha.Value : DBNull.Value;
                cmd.Parameters.Add("p_imagen",         OracleDbType.Varchar2).Value =
                    dto.Imagen ?? (object)DBNull.Value;

                var pIdOut = new OracleParameter("p_id_out", OracleDbType.Int32)
                    { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                    { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pIdOut);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var idOut  = Convert.ToInt32(pIdOut.Value.ToString());
                var mensaje = pMensaje.Value.ToString()!;

                if (idOut == -1)
                    return BadRequest(new RespuestaDto { Exito = false, Mensaje = mensaje });

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = mensaje,
                    Data    = new { IdCarnet = idOut }
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

        // ============================================================
        // DELETE api/imc/carnet/{id}
        // Eliminar carnet escaneado
        // ============================================================
        [HttpDelete("carnet/{id}")]
        public async Task<IActionResult> EliminarCarnet(int id)
        {
            try
            {
                var carnet = await _context.Carnetescaneados
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (carnet == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Carnet no encontrado."
                    });

                _context.Carnetescaneados.Remove(carnet);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Carnet eliminado correctamente."
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