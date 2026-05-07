using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Security.Claims;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Types;
namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistorialController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistorialController(AppDbContext context)
        {
            _context = context;
        }

      private int ObtenerIdUsuario() =>
            Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt
                            .JwtRegisteredClaimNames.Sub));

        // ============================================================
        // GET api/historial/{idMiembro}
        // Historial completo de vacunación de un miembro
        // ============================================================
        [HttpGet("{idMiembro}")]
        public async Task<IActionResult> ObtenerHistorial(int idMiembro)
        {
            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_obtener_historial_completo";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id_miembro", OracleDbType.Int32).Value = idMiembro;

                var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                    { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                    { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCursor);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var mensaje  = pMensaje.Value.ToString()!;
                var historial = new List<HistorialDto>();

                using var reader = ((OracleRefCursor)pCursor.Value).GetDataReader();

                while (await reader.ReadAsync())
                {
                    historial.Add(new HistorialDto
                    {
                        Id               = Convert.ToInt32(reader["ID"]),
                        FechaAplicacion  = Convert.ToDateTime(reader["FECHA_APLICACION"]),
                        ProximaDosis     = reader["PROXIMA_DOSIS"] == DBNull.Value
                                            ? null : Convert.ToDateTime(reader["PROXIMA_DOSIS"]),
                        DosisNumero      = Convert.ToInt32(reader["DOSIS_NUMERO"]),
                        Lote             = reader["LOTE"] == DBNull.Value
                                            ? null : reader["LOTE"].ToString(),
                        NombreMedico     = reader["NOMBRE_MEDICO"] == DBNull.Value
                                            ? null : reader["NOMBRE_MEDICO"].ToString(),
                        Observaciones    = reader["OBSERVACIONES"] == DBNull.Value
                                            ? null : reader["OBSERVACIONES"].ToString(),
                        Vacuna           = reader["VACUNA"].ToString()!,
                        Fabricante       = reader["FABRICANTE"] == DBNull.Value
                                            ? null : reader["FABRICANTE"].ToString(),
                        TipoVacuna       = reader["TIPO_VACUNA"] == DBNull.Value
                                            ? null : reader["TIPO_VACUNA"].ToString(),
                        Centro           = reader["CENTRO"] == DBNull.Value
                                            ? null : reader["CENTRO"].ToString(),
                        CentroDireccion  = reader["CENTRO_DIRECCION"] == DBNull.Value
                                            ? null : reader["CENTRO_DIRECCION"].ToString(),
                        RecordatorioEstado = reader["RECORDATORIO_ESTADO"] == DBNull.Value
                                            ? null : reader["RECORDATORIO_ESTADO"].ToString(),
                        FechaRecordatorio  = reader["FECHA_RECORDATORIO"] == DBNull.Value
                                            ? null : Convert.ToDateTime(reader["FECHA_RECORDATORIO"])
                    });
                }

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = mensaje,
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
        // POST api/historial
        // Registrar nueva vacunación
        // El trigger calcula proxima_dosis y crea el recordatorio solo
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> RegistrarVacunacion([FromBody] RegistrarVacunacionDto dto)
        {
            if (dto.IdMiembro == 0 || dto.IdVacuna == 0 || dto.DosisNumero == 0)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "IdMiembro, IdVacuna y DosisNumero son obligatorios."
                });
            }

            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_registrar_vacunacion";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id_miembro",       OracleDbType.Int32).Value    = dto.IdMiembro;
                cmd.Parameters.Add("p_id_vacuna",        OracleDbType.Int32).Value    = dto.IdVacuna;
                cmd.Parameters.Add("p_id_centro",        OracleDbType.Int32).Value    =
                    dto.IdCentro.HasValue ? dto.IdCentro.Value : DBNull.Value;
                cmd.Parameters.Add("p_fecha_aplicacion", OracleDbType.Date).Value     = dto.FechaAplicacion;
                cmd.Parameters.Add("p_dosis_numero",     OracleDbType.Int32).Value    = dto.DosisNumero;
                cmd.Parameters.Add("p_lote",             OracleDbType.Varchar2).Value =
                    dto.Lote ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_nombre_medico",    OracleDbType.Varchar2).Value =
                    dto.NombreMedico ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_observaciones",    OracleDbType.Varchar2).Value =
                    dto.Observaciones ?? (object)DBNull.Value;

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
                {
                    return BadRequest(new RespuestaDto { Exito = false, Mensaje = mensaje });
                }

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = mensaje,
                    Data    = new { IdHistorial = idOut }
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
        // GET api/historial/proximas-dosis
        // Próximas dosis pendientes de todos los miembros del usuario
        // ============================================================
        [HttpGet("proximas-dosis")]
        public async Task<IActionResult> ProximasDosis()
        {
            var idUsuario = ObtenerIdUsuario();
            var conn      = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_verificar_proxima_dosis";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = idUsuario;

                var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                    { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                    { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCursor);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var proximas = new List<ProximaDosisDto>();

                using var reader = ((OracleRefCursor)pCursor.Value).GetDataReader();

                while (await reader.ReadAsync())
                {
                    proximas.Add(new ProximaDosisDto
                    {
                        Miembro       = reader["MIEMBRO"].ToString()!,
                        TipoMiembro   = reader["TIPO_MIEMBRO"].ToString()!,
                        Vacuna        = reader["VACUNA"].ToString()!,
                        DosisAplicada = Convert.ToInt32(reader["DOSIS_APLICADA"]),
                        ProximaDosis  = Convert.ToDateTime(reader["PROXIMA_DOSIS"]),
                        DiasRestantes = reader["DIAS_RESTANTES"] == DBNull.Value
                                        ? null : Convert.ToInt32(reader["DIAS_RESTANTES"]),
                        Recordatorio  = reader["RECORDATORIO"] == DBNull.Value
                                        ? null : reader["RECORDATORIO"].ToString()
                    });
                }

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = proximas
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